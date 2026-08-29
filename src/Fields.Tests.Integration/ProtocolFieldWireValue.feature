Feature: Protocol Field wire values

  Scenario: Serializing a valid wire value
    Given a Protocol Field with wire value "GD92"
    When the Protocol Field is serialized
    Then the serialized wire value is "GD92"
