Feature: Mobilise messages

  Scenario: Creating and decoding a single-incident mobilisation envelope
    Given an Envelope source and destination of Brigade 26, Node 100, and Port 25
    And an Envelope priority of 1 and protocol version of 2
    And an Envelope sequence number of 31953 requesting acknowledgement
    And a single-block Mobilise Message for resource "A1" submitted at "07SEP26154309" for Incident 1
    When the Envelope is created and decoded
    Then its decoded Mobilise Message preserves the resource and incident details
    And its complete Envelope bytes are "1A191912011A191912FCD102010100303753455032363135343330390102413100000001010753544154494F4E01310B4849474820535452454554000000000008535531323334353604303132330004464952454A"
