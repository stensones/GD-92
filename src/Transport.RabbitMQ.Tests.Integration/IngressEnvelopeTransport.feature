Feature: Ingress Envelope Transport

  Scenario: Rejecting an encoded Envelope with trailing bytes
    Given Local Participant Ingress receives an encoded Envelope with a trailing byte
    When it decodes the ingress transport message
    Then Ingress Envelope Transport rejects the trailing byte
