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

  Scenario: Storing a first direct Non-Volatile Parameter value
    Given an empty isolated Router database
    When Router stores Non-Volatile Parameter 1 with value 25
    Then its Permanent Parameter 1 is absent
    And its Non-Volatile Parameter 1 has value 25

  Scenario: Storing a first direct Permanent Parameter value
    Given an empty isolated Router database
    When Router stores Permanent Parameter 1 with value 25
    Then its Permanent Parameter 1 has value 25
    And its Non-Volatile Parameter 1 is absent

  Scenario: Concurrently starting Router with different bootstrap configurations
    Given an empty isolated Router database
    When two Router Parameter starts use different bootstrap configurations concurrently
    Then both Router starts project one complete first-committed Parameter set
    And both Router starts use the first-committed Level 1 password verifier

  Scenario: Concurrently changing an existing Non-Volatile Parameter value
    Given an empty isolated Router database
    When two Router stores change existing Non-Volatile Parameter 1 concurrently
    Then one Router store reports a concurrent Parameter Store update
