Feature: Negative Acknowledgements

  Scenario: Creating a Negative Acknowledgement for one invalid message destination
    Given a Negative Acknowledgement destination Brigade 26, Node 100, Port 25
    And the General Reason Code "inv_mess"
    When a Negative Acknowledgement is created
    Then its Negative Acknowledgement Message Type bytes are "33"
    And its Negative Acknowledgement Contents bytes are "011A19190103"
