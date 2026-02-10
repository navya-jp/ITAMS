# Apache JMeter Performance Testing Guide for ITAMS

## Quick Configuration Reference

### Basic HTTP Request Settings

**Protocol:** `http`  
**Server Name or IP:** `localhost`  
**Port Number:** `5066`  
**Method:** Varies by endpoint (GET, POST, PUT, DELETE)

---

## 1. Thread Group Configuration

Right-click on Test Plan → Add → Threads (Users) → Thread Group

**Settings:**
- **Number of Threads (users):** `10` (start small, increase gradually)
- **Ramp-Up Period (seconds):** `10` (time to start all threads)
- **Loop Count:** `5` (or check "Infinite" for continuous testing)
- **Duration (seconds):** `60` (optional, for time-based tests)

**Recommended Test Scenarios:**
- **Light Load:** 10 users, 10s ramp-up, 5 loops
- **Medium Load:** 50 users, 30s ramp-up, 10 loops
- **Heavy Load:** 100 users, 60s ramp-up, 20 loops
- **Stress Test:** 200+ users, 120s ramp-up, continuous

---

## 2. HTTP Request Defaults

Right-click on Thread Group → Add → Config Element → HTTP Request Defaults

**Settings:**
- **Protocol:** `http`
- **Server Name or IP:** `localhost`
- **Port Number:** `5066`

---

## 3. HTTP Header Manager

Right-click on Thread Group → Add → Config Element → HTTP Header Manager

**Add these headers:**
```
Content-Type: application/json
Accept: application/json
```

**For authenticated requests, add:**
```
Authorization: Bearer ${token}
```

---

## 4. Test Endpoints Configuration

### A. Login/Authentication Test

**HTTP Request Settings:**
- **Name:** `Login - Get Token`
- **Method:** `POST`
- **Path:** `/api/auth/login`
- **Body Data (Body Data tab):**
```json
{
  "username": "superadmin",
  "password": "Admin@123"
}
```

**Add JSON Extractor (to capture token):**
- Right-click on Login request → Add → Post Processors → JSON Extractor
  - **Names of created variables:** `token`
  - **JSON Path expressions:** `$.token`
  - **Match No.:** `1`
  - **Default Values:** `TOKEN_NOT_FOUND`

---

### B. Get Users Test

**HTTP Request Settings:**
- **Name:** `Get All Users`
- **Method:** `GET`
- **Path:** `/api/users`

**Add HTTP Header Manager (specific to this request):**
```
Authorization: Bearer ${token}
```

---

### C. Create User Test

**HTTP Request Settings:**
- **Name:** `Create User`
- **Method:** `POST`
- **Path:** `/api/users`
- **Body Data:**
```json
{
  "username": "testuser${__Random(1,10000)}",
  "email": "test${__Random(1,10000)}@example.com",
  "firstName": "Test",
  "lastName": "User",
  "roleId": 3,
  "password": "Test@123456",
  "mustChangePassword": true
}
```

---

### D. Get RBAC Roles Test

**HTTP Request Settings:**
- **Name:** `Get RBAC Roles`
- **Method:** `GET`
- **Path:** `/api/rbac/roles`

---

### E. Get RBAC Permissions Test

**HTTP Request Settings:**
- **Name:** `Get RBAC Permissions`
- **Method:** `GET`
- **Path:** `/api/rbac/permissions`

---

### F. Get Projects Test

**HTTP Request Settings:**
- **Name:** `Get All Projects`
- **Method:** `GET`
- **Path:** `/api/superadmin/projects`

---

### G. Get Locations Test

**HTTP Request Settings:**
- **Name:** `Get All Locations`
- **Method:** `GET`
- **Path:** `/api/superadmin/locations`

---

## 5. Listeners (Results/Reports)

Add these listeners to view results:

### View Results Tree
Right-click on Thread Group → Add → Listener → View Results Tree
- Shows individual request/response details
- Good for debugging

### Summary Report
Right-click on Thread Group → Add → Listener → Summary Report
- Shows aggregate statistics
- Throughput, average response time, error %

### Aggregate Report
Right-click on Thread Group → Add → Listener → Aggregate Report
- Detailed statistics with percentiles
- Min, Max, Median, 90th, 95th, 99th percentiles

### Graph Results
Right-click on Thread Group → Add → Listener → Graph Results
- Visual representation of response times

### Response Time Graph
Right-click on Thread Group → Add → Listener → Response Time Graph
- Shows response time trends over time

---

## 6. Assertions (Validation)

Add assertions to validate responses:

### Response Assertion
Right-click on HTTP Request → Add → Assertions → Response Assertion

**For successful requests:**
- **Field to Test:** `Response Code`
- **Pattern Matching Rules:** `Equals`
- **Patterns to Test:** `200`

**For JSON responses:**
- **Field to Test:** `Text Response`
- **Pattern Matching Rules:** `Contains`
- **Patterns to Test:** `"success":true`

### JSON Assertion
Right-click on HTTP Request → Add → Assertions → JSON Assertion
- **Assert JSON Path exists:** `$.success`
- **Expected Value:** `true`

---

## 7. Timers (Think Time)

Add timers to simulate realistic user behavior:

### Constant Timer
Right-click on Thread Group → Add → Timer → Constant Timer
- **Thread Delay (milliseconds):** `1000` (1 second between requests)

### Uniform Random Timer
Right-click on Thread Group → Add → Timer → Uniform Random Timer
- **Random Delay Maximum (milliseconds):** `2000`
- **Constant Delay Offset (milliseconds):** `500`
- Results in delays between 500-2500ms

---

## 8. Complete Test Scenario Example

### Realistic User Flow Test Plan

**Structure:**
```
Test Plan
└── Thread Group (50 users, 30s ramp-up, 10 loops)
    ├── HTTP Request Defaults (localhost:5066)
    ├── HTTP Header Manager (Content-Type, Accept)
    ├── Uniform Random Timer (500-2500ms)
    │
    ├── 1. Login Request (POST /api/auth/login)
    │   └── JSON Extractor (extract token)
    │
    ├── 2. Get Users (GET /api/users)
    │   ├── HTTP Header Manager (Authorization: Bearer ${token})
    │   └── Response Assertion (200 OK)
    │
    ├── 3. Get RBAC Roles (GET /api/rbac/roles)
    │   ├── HTTP Header Manager (Authorization: Bearer ${token})
    │   └── Response Assertion (200 OK)
    │
    ├── 4. Get Projects (GET /api/superadmin/projects)
    │   ├── HTTP Header Manager (Authorization: Bearer ${token})
    │   └── Response Assertion (200 OK)
    │
    ├── 5. Get Locations (GET /api/superadmin/locations)
    │   ├── HTTP Header Manager (Authorization: Bearer ${token})
    │   └── Response Assertion (200 OK)
    │
    └── Listeners
        ├── View Results Tree
        ├── Summary Report
        ├── Aggregate Report
        └── Response Time Graph
```

---

## 9. CSV Data Set Config (For Multiple Users)

Create a file `users.csv`:
```csv
username,password
superadmin,Admin@123
john.doe,Test@123
testuser,Test@123
```

**Add CSV Data Set Config:**
Right-click on Thread Group → Add → Config Element → CSV Data Set Config
- **Filename:** `users.csv`
- **Variable Names:** `username,password`
- **Delimiter:** `,`
- **Recycle on EOF:** `True`
- **Stop thread on EOF:** `False`
- **Sharing mode:** `All threads`

**Update Login Request Body:**
```json
{
  "username": "${username}",
  "password": "${password}"
}
```

---

## 10. Performance Metrics to Monitor

### Key Metrics:
1. **Response Time:**
   - Average: < 500ms (Good), < 1000ms (Acceptable)
   - 90th Percentile: < 1000ms
   - 95th Percentile: < 2000ms

2. **Throughput:**
   - Requests per second
   - Higher is better

3. **Error Rate:**
   - Should be < 1%
   - 0% is ideal

4. **Concurrent Users:**
   - Maximum users system can handle
   - Before response time degrades

---

## 11. Test Execution Steps

1. **Start Backend Server:**
   ```bash
   dotnet run
   ```
   Ensure it's running on `http://localhost:5066`

2. **Open JMeter:**
   - Launch Apache JMeter

3. **Create Test Plan:**
   - Follow the structure above
   - Add Thread Group, HTTP Requests, Listeners

4. **Save Test Plan:**
   - File → Save Test Plan As → `ITAMS_Performance_Test.jmx`

5. **Run Test:**
   - Click green "Start" button (Play icon)
   - Or: Run → Start

6. **Monitor Results:**
   - Watch listeners for real-time results
   - Check for errors in View Results Tree

7. **Analyze Results:**
   - Review Summary Report
   - Check Aggregate Report for percentiles
   - Look at Response Time Graph for trends

8. **Generate HTML Report (CLI):**
   ```bash
   jmeter -n -t ITAMS_Performance_Test.jmx -l results.jtl -e -o report
   ```

---

## 12. Common JMeter Functions

Use these in your requests:

- **Random Number:** `${__Random(1,1000)}`
- **Current Time:** `${__time(yyyy-MM-dd HH:mm:ss)}`
- **UUID:** `${__UUID()}`
- **Counter:** `${__counter(TRUE)}`
- **Random String:** `${__RandomString(10,abcdefghijklmnopqrstuvwxyz)}`

---

## 13. Troubleshooting

### Common Issues:

**Connection Refused:**
- Ensure backend is running on port 5066
- Check firewall settings

**401 Unauthorized:**
- Verify token extraction is working
- Check Authorization header format

**High Error Rate:**
- Reduce number of threads
- Increase ramp-up period
- Check server logs for errors

**Slow Response Times:**
- Check database performance
- Monitor server CPU/Memory
- Optimize database queries

---

## 14. Best Practices

1. **Start Small:** Begin with 1-5 users, verify everything works
2. **Increase Gradually:** Slowly increase load to find breaking point
3. **Use Realistic Data:** Use CSV files with real test data
4. **Add Think Time:** Use timers to simulate real user behavior
5. **Monitor Server:** Watch CPU, memory, database during tests
6. **Run Multiple Times:** Run tests multiple times for consistency
7. **Test Different Scenarios:** Login, CRUD operations, reports
8. **Clean Up:** Remove test data after performance testing

---

## 15. Sample Test Results Interpretation

### Good Performance:
```
Average Response Time: 250ms
90th Percentile: 450ms
95th Percentile: 600ms
Error Rate: 0%
Throughput: 100 req/sec
```

### Needs Optimization:
```
Average Response Time: 2000ms
90th Percentile: 4000ms
95th Percentile: 6000ms
Error Rate: 5%
Throughput: 20 req/sec
```

---

## Quick Start Checklist

- [ ] Backend running on localhost:5066
- [ ] JMeter installed and opened
- [ ] Test Plan created
- [ ] Thread Group added (10 users, 10s ramp-up)
- [ ] HTTP Request Defaults configured
- [ ] Login request added with JSON Extractor
- [ ] At least 3-5 API endpoints added
- [ ] Listeners added (View Results Tree, Summary Report)
- [ ] Test plan saved
- [ ] Test executed successfully
- [ ] Results analyzed

---

*Generated: 2026-02-09*
*ITAMS Performance Testing Guide*
