Feature: Communications Address fields

  Scenario: Creating a Communications Address from envelope values
    Given valid Brigade 26, Node 100, and Port 25 values
    When a Communications Address is created
    Then its serialized field bytes are "1A1919"
