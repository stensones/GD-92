Feature: Router Password Verifier Persistence

  Scenario: Reloading a stored Level 1 password verifier
    Given an empty isolated Router password verifier database
    And a persistent Router permanent Parameter Set
    When I store a Level 1 password verifier in Permanent Parameter Table at Parameter 5
    Then a fresh Router password verifier store retrieves a verifier that accepts the original password
    And it rejects a different password
