Feature: Router Parameter Bootstrap

  Scenario: Loading the configured brigade or agency number after a Router restart
    Given an empty isolated Router database
    When Router Parameter startup loads brigade or agency number 26
    Then its current Parameter 1 is brigade or agency number 26
    When Router Parameter startup runs again
    Then its current Parameter 1 is brigade or agency number 26

  Scenario: Bootstrapping the initial typed Router Parameters
    Given an empty isolated Router database
    When Router Parameter startup bootstraps its initial Parameters
    Then its current Password is the neutral local Router Password Parameter
    And its current No Acknowledgement Timeout is 5 seconds
    And its current Retries value is 3
    And its Level 1 Password has permanent and non-volatile verifiers but no opaque Parameter values
