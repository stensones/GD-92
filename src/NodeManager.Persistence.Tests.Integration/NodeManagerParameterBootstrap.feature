Feature: NodeManager Parameter Bootstrap

  Scenario: Storing a first direct Non-Volatile Parameter value
    Given an empty isolated NodeManager database
    When NodeManager stores Non-Volatile Parameter 1 with value 25
    Then its Permanent Parameter 1 is absent
    And its Non-Volatile Parameter 1 has value 25

  Scenario: Storing a first direct Permanent Parameter value
    Given an empty isolated NodeManager database
    When NodeManager stores Permanent Parameter 1 with value 25
    Then its Permanent Parameter 1 has value 25
    And its Non-Volatile Parameter 1 is absent

  Scenario: Concurrently starting NodeManager with different bootstrap configurations
    Given an empty isolated NodeManager database
    When two NodeManager Parameter starts use different bootstrap configurations concurrently
    Then both NodeManager starts project one complete first-committed Parameter set

  Scenario: Concurrently changing an existing Non-Volatile Parameter value
    Given an empty isolated NodeManager database
    When two NodeManager stores change existing Non-Volatile Parameter 1 concurrently
    Then one NodeManager store reports a concurrent Parameter Store update
