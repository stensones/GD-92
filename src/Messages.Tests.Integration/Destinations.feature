Feature: Envelope destinations

  Scenario: Creating multiple Envelope destinations
    Given destination addresses Brigade 26, Node 100, Port 25 and Brigade 26, Node 101, Port 26
    When Envelope destinations are created
    Then their destination bytes are "1A19191A195A"
    And their Destination Count is 2
