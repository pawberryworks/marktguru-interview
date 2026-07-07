## Tasks

---

### Task 1 — Fix the existing issues

#### Test **GetActiveOffersAsync_ReturnsAllSeedOffers**
In production code there is used var now = DateTime.UtcNow; which is incorrect. For testing purposes, we need to use a fixed date and time to ensure consistent results. I've updated the code to use a fixed date and time for testing.
1. Created a new interface IDateTimeProvider with a variable UtcNow.
2. Implemented a class SystemDateTimeProvider that returns the current UTC date and time for production code.
3. Implemented a class FakeDateTimeProvider that returns a fixed UTC date and time for testing purposes.

#### Test **ImportOffers_ReturnsImportedCount_InResponseBody**
I've had to modify ImportAsync method in OfferImportService. I've change return value from void to Task<int> where we are returning count of rows. But overall I have another idea so I've put comment to the test. We can discuss it later. 

#### Test **ImportOffers_Returns500_WhenProductDoesNotExist**
I've had again to modify ImportAsync method in OfferImportService. I've added exception handling for the case when a product does not exist. The method now throws a specific exception that can be caught in the test to verify that a 500 status code is returned.

---

### Task 2 — Extend the schema

#### Steps:
1. Create a class with variables requested in the task description
2. Modify of Offer to add there key to Retailer class
3. Add modelBuilder.Entity<Retailer> and Seed in DbContext
4. Create new migration
5. Test migration

---

### Task 3 — New API endpoints

#### `GET /api/offers`
1.	Added OfferDto and PaginatedOffersResponse DTOs.
2.	Implemented IOfferRepository.GetPaginatedOffersAsync(...) with EF projection including baseQuery.Count() in the projection.
3.	OfferService delegates to repository and controller only validates/handles exceptions and returns proper HTTP results.
4.	Added integration tests (happy path with data, empty result set, invalid pageSize).

#### Tests added
-	GetOffers_ReturnsPaginatedResult_WithData: asserts items, total, page, pageSize and item fields.
-	GetOffers_ReturnsEmptyResult_WhenNoMatches: productId filter with no matches returns total=0 and empty items.
-	GetOffers_ReturnsBadRequest_WhenPageSizeTooLarge: pageSize > 100 returns 400.

#### `GET /api/retailers/{id}/stats`
1.	Added IRetailerRepository and implemented relational projection to compute counts, averages, min, and most recent offer in a single query.
2.	RetailersController maps KeyNotFoundException -> 404.
3.	Added unit/integration coverage for endpoint behavior used by other tests.

#### `POST /api/retailers`
1.	Added RetailerCreateDto with DataAnnotations for validation.
2.	Implemented IRetailerService.CreateRetailerAsync and RetailerService.CreateRetailerAsync to check duplicates and save retailer (returns new id).
3.	Controller returns Created(...) and maps DuplicateRetailerException -> 409.
4.	Added integration tests for success, duplicate name, and invalid country code.

#### Tests added
-	CreateRetailer_ReturnsCreated_WithLocationHeader: posts valid DTO, asserts 201, Location header, and created resource reachable.
-	CreateRetailer_ReturnsConflict_WhenDuplicateName: posts existing name, asserts 409.
-	CreateRetailer_ReturnsBadRequest_WhenInvalidCountry: posts invalid country length, asserts 400.

---

### Task 4 — Background job

#### Requirements implemented
1.	The job finds offers where ValidTo < UTC now AND Status != Expired.
2.	Performs a single bulk UPDATE via EF Core ExecuteUpdateAsync so the database receives one UPDATE statement.
3.	Logs: "Expired {n} offers at {timestamp}" (UTC ISO).
4.	Idempotent: predicate excludes already-expired rows; repeated runs produce same result.
5.	Proper async/await and CancellationToken support.
6.	Hosted via Hangfire recurring job with Cron.Hourly registration in Program.cs.

#### Unit test OfferExpiryJobTests:
-	Uses a shared in-memory SQLite connection and real EF relational translation.
-	Seeds offers with combinations: (ValidTo < now && Status != Expired), (ValidTo < now && Status == Expired), (ValidTo >= now && Status != Expired), etc.
-	Calls OfferExpiryJob.ExecuteAsync(ct) and asserts:
-	Only offers satisfying both conditions are updated to Expired.
-	The returned/observed count of changed rows matches the number of affected offers.
-	Uses Sqlite provider so ExecuteUpdateAsync is supported and behavior reflects production SQL.

---

### Future ideas:
1.	Centralize data access behind repository interfaces (finish/extend)
-	Why: isolates EF, simplifies testing, enables reuse (you started this).
-	How: add IOfferRepository/IRetailerRepository methods for all complex queries and bulk ops; move EF logic there.

2.	Move DTO mapping to a single mapper layer (AutoMapper or small mappers)
-	Why: consistent mapping, easier changes, avoids hand-written mapper duplication and serialization surprises.
-	How: add AutoMapper profiles or small mapping extension methods and use in services.

3.	Add FluentValidation + ProblemDetails error handling
-	Why: consistent input validation and standardized error responses (400 + problem details).
-	How: add validators for DTOs, register middleware to convert exceptions to ProblemDetails responses.

4.	Harden API surface & contracts
-	Why: predictable responses, better backwards compatibility.
-	How: add API versioning, explicit response types, OpenAPI docs with examples for each endpoint.

5.	Add more unit tests and edge-case coverage
-	Why: reduce regressions (nulls, empty datasets, boundary paging, invalid enums).
-	How: add tests for OfferService pagination edge cases, RetailerService averaging/min when no offers, import error paths.

---

### Time

**Without AI:** Around 7-8 hours

**With AI:** Around 4 hours

#### What I've calculated:
| Without AI | With AI |
| --- | --- |
| Read and fully understand README.md | Read and fully understand README.md |
| Read and understand project | Ask AI for summary with the most important key points |
| Write code | Ask AI for write code | 
| Commit & push | Do whole PR review with changes made by AI - I had to fully understand AI code, be sure that meets standards and quality |
| Merge | Merge |
| Write Solution.md by myself | Ask AI for a help with finding better words |

#### Summary 
AI shorten the time needed for this task, but what's important, some of the elements could not be made by AI and some elements needed to be checked by human after AI. 