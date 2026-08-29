Feature: Message Types

  Scenario: Creating the Text message type field
    Given the Text GD92 message type
    When a Message Type field is created
    Then its field bytes are "1B"
