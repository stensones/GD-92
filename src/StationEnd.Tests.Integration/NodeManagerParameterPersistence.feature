Feature: NodeManager Parameter Persistence

  Scenario: Bootstrapping and retaining NodeManager User Agent identity Parameters
    Given an empty dedicated NodeManager Parameter Store
    And NodeManager is the User Agent at Brigade 26, Node 100, and Port 25
    And its local Router is at Brigade 26, Node 100, and Port 0
    When a remote User Agent requests NodeManager Current Parameter 1
    Then NodeManager returns port number 25
    When a remote User Agent requests NodeManager Current Parameter 2
    Then NodeManager returns Network Management User Agent type 12
    When NodeManager restarts
    And a remote User Agent requests NodeManager Current Parameter 1
    Then NodeManager returns port number 25
    When a remote User Agent requests NodeManager Current Parameter 2
    Then NodeManager returns Network Management User Agent type 12
    And NodeManager retains port number 25 and Network Management User Agent type 12 in its Permanent and Non-Volatile Parameter Tables
