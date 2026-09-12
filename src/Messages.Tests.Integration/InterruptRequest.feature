Feature: Interrupt requests

  Scenario: Creating and decoding an emergency interrupt request envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 2 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And an Interrupt Request from resource "A1" declaring an Emergency with text "MAYDAY"
    When the Envelope is created and decoded
    Then its decoded Interrupt Request identifies resource "A1", declares an Emergency, and contains text "MAYDAY"
    And its complete Envelope bytes are "1A191903011A191922FCD1190241314500064D41594441592C"
