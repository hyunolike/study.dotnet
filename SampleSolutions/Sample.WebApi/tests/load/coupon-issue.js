import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://host.docker.internal:5000';
const COUPON_ID = __ENV.COUPON_ID || 'WELCOME10';
const USER_PREFIX = __ENV.USER_PREFIX || 'load-user';
const PAUSE_SECONDS = Number(__ENV.PAUSE_SECONDS || 0.1);

const STOCK_LIMIT = Number(__ENV.STOCK_LIMIT || 100);
const OVERSUBSCRIBE_FACTOR = Number(__ENV.OVERSUBSCRIBE_FACTOR || 1.2);
const DURATION_SECONDS = Number(__ENV.DURATION_SECONDS || 20);
const TOTAL_REQUESTS = Math.max(1, Math.ceil(STOCK_LIMIT * OVERSUBSCRIBE_FACTOR));
const REQUEST_RATE =
  __ENV.REQUEST_RATE !== undefined
    ? Math.max(1, Number(__ENV.REQUEST_RATE))
    : Math.max(1, Math.ceil(TOTAL_REQUESTS / DURATION_SECONDS));

const PREALLOCATED_VUS = Number(
  __ENV.PREALLOCATED_VUS || Math.max(10, Math.min(REQUEST_RATE * 2, 100)),
);
const MAX_VUS = Number(__ENV.MAX_VUS || Math.max(PREALLOCATED_VUS, REQUEST_RATE * 3));

export const options = {
  scenarios: {
    limited_stock_push: {
      executor: 'constant-arrival-rate',
      rate: REQUEST_RATE,
      timeUnit: '1s',
      duration: `${DURATION_SECONDS}s`,
      preAllocatedVUs: PREALLOCATED_VUS,
      maxVUs: MAX_VUS,
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<500'],
  },
};

export function setup() {
  const plannedRequests = REQUEST_RATE * DURATION_SECONDS;
  console.log(
    `[k6] Planned load: ${plannedRequests} iterations (~${TOTAL_REQUESTS} target vs stock ${STOCK_LIMIT}, ${REQUEST_RATE}/s for ${DURATION_SECONDS}s)`,
  );
}

const issueAttempts = new Counter('issue_attempts');
const issueConflicts = new Counter('issue_conflicts');
const issueSuccess = new Counter('issue_success');

export default function issueCoupon() {
  const userId = `${USER_PREFIX}-${__VU}-${Date.now()}`;
  const payload = JSON.stringify({ couponId: COUPON_ID, userId });

  const res = http.post(`${BASE_URL}/coupons/issue`, payload, {
    headers: { 'Content-Type': 'application/json' },
    timeout: __ENV.TIMEOUT || '3s',
  });

  issueAttempts.add(1);

  const ok = check(res, {
    'status is 200': (r) => r.status === 200,
    'status is 409': (r) => r.status === 409,
  });

  if (res.status === 200) {
    issueSuccess.add(1);
  }

  if (res.status === 409) {
    issueConflicts.add(1);
  }

  if (!ok) {
    console.error(`Unexpected response ${res.status}: ${res.body}`);
  }

  sleep(PAUSE_SECONDS);
}
