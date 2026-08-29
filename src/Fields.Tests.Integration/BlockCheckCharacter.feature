Feature: Block Check Characters

  Scenario: Calculating an Envelope Block Check Character
    Given the Envelope bytes before BCC are "1A191902011A191912FCD11B0101000446495245"
    When the Block Check Character is calculated
    Then its Block Check Character field bytes are "3B"
