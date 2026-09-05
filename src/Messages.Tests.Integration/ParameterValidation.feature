Feature: Parameter Contents validation

  Scenario: Rejecting a Parameter Value that is not byte-aligned
    Given Parameter Contents with a partial Parameter Value byte
    When Parameter Contents decoding is attempted
    Then the partial Parameter Value is rejected
