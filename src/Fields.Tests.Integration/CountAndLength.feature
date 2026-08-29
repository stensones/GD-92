Feature: Count and length fields

  Scenario: Creating count and length for a Text message
    Given a message length of 8 bytes and 1 destination
    When a CountAndLength field is created
    Then its count and length field bytes are "0201"
