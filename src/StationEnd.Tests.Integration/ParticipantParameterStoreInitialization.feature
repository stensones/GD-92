Feature: Participant Parameter Store Initialization

  Scenario: Retaining one complete first-committed Participant Parameter Store initialization
    Given empty Router and NodeManager Participant Parameter Stores
    When two Router starts use different bootstrap configurations concurrently
    And two NodeManager starts use different bootstrap configurations concurrently
    Then each Router Current Parameter Table contains one complete first-committed Router Parameter set
    And each NodeManager Current Parameter Table contains one complete first-committed NodeManager Parameter set
    And each Router Current Parameter Table uses the first-committed Level 1 password verifier
