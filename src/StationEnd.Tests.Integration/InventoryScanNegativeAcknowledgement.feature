Feature: Inventory Scan Negative Acknowledgement

  Scenario: Recording a rejected Inventory Scan probe
    Given local participant port 3 rejects its Inventory Scan probe with invalid syntax
    When NodeManager starts an Inventory Scan
    Then the completed Inventory Scan summary records one invalid_syntax Negative Acknowledgement
    And the completed Inventory Scan summary records 62 timeouts
