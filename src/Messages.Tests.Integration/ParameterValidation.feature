Feature: Parameter Contents validation

  Scenario: Rejecting a Parameter Value that is not byte-aligned
    Given Parameter Contents with a partial Parameter Value byte
    When Parameter Contents decoding is attempted
    Then the partial Parameter Value is rejected

  Scenario: Rejecting a Set Parameter Value that is not byte-aligned
    Given Set Parameter Contents with a partial Parameter Value byte
    When Set Parameter Contents decoding is attempted
    Then the partial Set Parameter Value is rejected
