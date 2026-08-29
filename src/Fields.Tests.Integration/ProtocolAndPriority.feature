Feature: Protocol and priority fields

  Scenario: Creating protocol and priority for an envelope
    Given valid Message Priority 1 and Protocol Version 2 values
    When a ProtocolAndPriority field is created
    Then its protocol and priority field bytes are "12"
