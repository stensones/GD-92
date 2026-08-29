Feature: Text messages

  Scenario: Creating a single-block text message
    Given Block 1 of 1 containing the text "FIRE"
    When a Text message is created
    Then its message bytes are "0101000446495245"

  Scenario: Identifying a text message
    Given Block 1 of 1 containing the text "FIRE"
    When a Text message is created
    Then its Message Type bytes are "1B"

  Scenario: Decoding a single-block Text message
    Given Text message contents bytes "0101000446495245"
    When the Text message contents are decoded
    Then its decoded block is 1
    And its decoded number of blocks is 1
    And its decoded text is "FIRE"
