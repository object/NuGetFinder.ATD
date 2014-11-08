Feature: Search packages

Scenario: Find 10 most polular packages
	Given I am on the Search page
	When search packages by criteria
	| Search text | Sort by    | Number of results |
	|             | Popularity | 10                |
	Then I should get 10 results
	And the top result should be "jQuery"

Scenario: Find 100 most recently updated packages
	Given I am on the Search page
	When search packages by criteria
	| Search text | Sort by     | Number of results |
	|             | Update time | 100               |
	Then I should get 100 results
	And the top result should have update time later than yesterday

Scenario: Find 25 SQL packages
	Given I am on the Search page
	When search packages by criteria
	| Search text | Sort by       | Number of results |
	| SQL         | Package title | 25                |
	Then I should get 25 results
	And all results should contain text "SQL" in the package title
