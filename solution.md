## Tasks

### Task 1 — Fix the existing issues

#### Test **GetActiveOffersAsync_ReturnsAllSeedOffers**
In production code there is used var now = DateTime.UtcNow; which is incorrect. For testing purposes, we need to use a fixed date and time to ensure consistent results. I've updated the code to use a fixed date and time for testing.
	1. Created a new interface IDateTimeProvider with a variable UtcNow.
	2. Implemented a class SystemDateTimeProvider that returns the current UTC date and time for production code.
	3. Implemented a class FakeDateTimeProvider that returns a fixed UTC date and time for testing purposes.

#### Test **ImportOffers_ReturnsImportedCount_InResponseBody**
I've had to modify ImportAsync method in OfferImportService. I've change return value from void to Task<bool> where we are returning count of rows. But overall I have another idea so I've put comment to the test. We can discuss it later. 


#### Test **ImportOffers_Returns500_WhenProductDoesNotExist**
I've had again to modify ImportAsync method in OfferImportService. I've added exception handling for the case when a product does not exist. The method now throws a specific exception that can be caught in the test to verify that a 500 status code is returned.