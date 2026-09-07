Feature: Deferred Parameter Request

  Scenario: Timing out after wait_ack without retransmission
    Given the local Router responds to a Parameter Request with wait_ack and no final response
    When an awaiting NodeManager user requests the local Router brigade or agency number
    Then NodeManager submits the Parameter Request only once
    And the deferred Parameter Request status eventually shows timed-out
