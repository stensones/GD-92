Feature: Encoded Message Buffer validation

  Scenario: Rejecting an unsigned read beyond the encoded payload
    Given an Encoded Message Buffer containing one octet
    When it attempts to read nine unsigned bits
    Then the read beyond the encoded payload is rejected
