Feature: Peripheral status

  Scenario: Creating and decoding an unsolicited peripheral status envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And an unsolicited Peripheral Status reporting manual acknowledgement, paper low, station sounders, and appliance indicator 1
    When the Envelope is created and decoded
    Then its decoded Peripheral Status reports manual acknowledgement, paper low, station sounders, and appliance indicator 1
    And its complete Envelope bytes are "1A191901011A191932FCD11C0021010122"
