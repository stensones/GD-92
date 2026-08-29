Feature: Word8 wire values

  Scenario: Round-tripping serialized Word8 data across a byte boundary
    Given encoded frame payload "E540"
    And the encoded message buffer is positioned at bit 3
    When a Word8 is created from the encoded message buffer
    And the Word8 is serialized
    Then the serialized Word8 data is "2A"
    When a new Word8 is created from the serialized data
    Then the new Word8 is value identical to the original Word8
    And the encoded message buffer is positioned at byte 1 and bit 3
