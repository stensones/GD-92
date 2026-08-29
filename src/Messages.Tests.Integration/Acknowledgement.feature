Feature: Acknowledgements

  Scenario: Creating an acknowledgement
    When an Acknowledgement is created
    Then its Message Type bytes are "32"
    And its Contents bytes are empty
