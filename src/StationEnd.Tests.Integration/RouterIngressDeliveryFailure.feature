Feature: Router Ingress delivery failure

  Scenario: Retaining an unsuccessful Router Ingress submission
    Given NodeManager's configured Router Ingress cannot submit a management Envelope
    When the NodeManager user requests the local Router brigade or agency number
    Then NodeManager redirects to the retained Parameter Request status
    And the Parameter Request status shows delivery-failed
