# Deliver local management Messages through Participant Ingress

GD-92 requires Message Transfer Agents and User Agents to receive management Parameter Requests, while leaving their local Router-to-participant interface unspecified. Router will deliver those Messages through a neutral Local Participant Ingress addressed by Communications Address, retaining User-Agent Ingress for User-Agent-specific delivery so an MTA is not modeled as a User Agent.
