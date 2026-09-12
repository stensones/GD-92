Feature: Proforma definition queries

  Scenario: Creating and decoding a proforma definition query envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 3 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a Proforma Definition Query for the text table format
    When the Envelope is created and decoded
    Then its decoded Proforma Definition Query identifies the text table format
