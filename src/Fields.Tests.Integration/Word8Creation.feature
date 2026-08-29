Feature: Creating Word8 fields

  Scenario: Creating a Word8 from a valid value
    Given the valid Word8 value 42
    When a Word8 is created from the value
    Then the Word8 serializes as "2A"
