Feature: Router Parameter Bootstrap

  Scenario: Loading the configured brigade or agency number after a Router restart
    Given an empty isolated Router database
    When Router Parameter startup loads brigade or agency number 26
    Then its current Parameter 1 is brigade or agency number 26
    When Router Parameter startup runs again
    Then its current Parameter 1 is brigade or agency number 26
