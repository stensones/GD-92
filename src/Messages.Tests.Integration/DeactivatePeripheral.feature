Feature: Deactivate peripherals

  Scenario: Creating and decoding a deactivate peripheral envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Deactivate Peripheral message deactivating station sounders and appliance indicator 1
    When the Envelope is created and decoded
    Then its decoded Deactivate Peripheral message deactivates station sounders and appliance indicator 1
    And its complete Envelope bytes are "1A191900811A191912FCD1080101B6"
