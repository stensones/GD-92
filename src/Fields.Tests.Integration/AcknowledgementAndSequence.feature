Feature: Acknowledgement and sequence fields

  Scenario: Creating an acknowledgement-requested sequence field
    Given Sequence Number 31953 and an Acknowledgement Request
    When an AcknowledgementAndSequence field is created
    Then its acknowledgement and sequence field bytes are "FCD1"
