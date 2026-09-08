Feature: Local Participant Ingress

  Scenario: Rejecting a management Envelope addressed to multiple local participants
    Given a local Router has a Local Participant Ingress
    When it delivers a management Envelope addressed to two local participants
    Then Local Participant Ingress rejects the Envelope before selecting an endpoint
