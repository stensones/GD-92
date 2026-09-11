Feature: Alert crews

  Scenario: Creating and decoding an alert crew envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And an Alert Crew message for Firecall Team A requiring manual acknowledgement and activating station sounders and appliance indicator 1
    When the Envelope is created and decoded
    Then its decoded Alert Crew message identifies Firecall Team A, requires manual acknowledgement, and activates station sounders and appliance indicator 1
    And its complete Envelope bytes are "1A191901411A191912FCD128464101010151"
