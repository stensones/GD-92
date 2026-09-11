Feature: Mobilise commands

  Scenario: Creating and decoding a mobilise command envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Mobilise Command activating station sounders and appliance indicator 1 requiring manual acknowledgement
    When the Envelope is created and decoded
    Then its decoded Mobilise Command activates station sounders and appliance indicator 1 and requires manual acknowledgement
    And its complete Envelope bytes are "1A191900C11A191912FCD101010101FE"
