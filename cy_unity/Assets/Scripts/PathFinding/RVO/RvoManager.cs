using System.Collections.Generic;
using FindingPath.Data;
using UnityEngine;

namespace RVO
{
    public interface IRvoAgentUnit
    {
        int AgentId { get; set; }
        Vector2 GoalPosition { get; }
        float Speed { get; }
        void UpdatePosition(Vector3 position);
    }
    
    public sealed class RvoManager
    {
        private static readonly float Step = 1 / 50f;

        private float timer;
        private int frameCount;
        private float delta;
        private System.Random random = new System.Random(0);

        public void Init(NavMesh navMesh)
        {
            Simulator.Instance.Clear();
            Simulator.Instance.setTimeStep(Step);
            Simulator.Instance.SetNumWorkers(4);
            Simulator.Instance.setAgentDefaults(15.0f, 15, 5.0f, 5.0f, 2.0f, 10.0f, new Vector2(0.0f, 0.0f));
            var obstacles = navMesh.RvoObstacles;
            foreach (var item in obstacles)
            {
                Simulator.Instance.addObstacle(item.vertices);
            }
            /*
             * Process the obstacles so that they are accounted for in the
             * simulation.
             */
            Simulator.Instance.processObstacles();
        }
        
        public void Update(float deltaTime)
        {
            timer += deltaTime;
            //delta += deltaTime;
            Simulator.Instance.setTimeStep(deltaTime);
            //if (timer > frameCount * Step)
            {
                frameCount++;
                UpdateAgents(deltaTime);
                Simulator.Instance.doStepSync();
                delta = 0;
            }
        }

        public int AddAgent(Vector3 position,float radius,IRvoAgentUnit unit)
        {
            var agent = Simulator.Instance.addAgent(new Vector2(position.x, position.z), radius, unit);
            unit.AgentId = agent;
            return agent;
        }

        public void RemoveAgent(IRvoAgentUnit unit)
        {
            var numUnit = Simulator.Instance.getNumAgents();
            for (int index = 0; index < numUnit; index++)
            {
                var agent = Simulator.Instance.agents_[index];
                if (agent.id_ == unit.AgentId)
                {
                    Simulator.Instance.agents_.RemoveAt(index);
                    Simulator.Instance.agentCountChanged = true;
                    return;
                }
            }
        }
        
        private void UpdateAgents(float deltaTime)
        {
            var numUnit = Simulator.Instance.getNumAgents();
            for (int i = 0; i < numUnit; ++i)
            {
                var agent = Simulator.Instance.agents_[i];
                Vector2 goalVector = agent.unit.GoalPosition - agent.position_;

                if (RVOMath.absSq(goalVector) > 1.0f)
                {
                    goalVector = RVOMath.normalize(goalVector) * agent.unit.Speed;
                }

                Simulator.Instance.setAgentVelocity(i, goalVector);
                Simulator.Instance.setAgentPrefVelocity(i, goalVector);
                agent.unit.UpdatePosition(new Vector3(agent.position_.x_,0,agent.position_.y_));
            }
        }
    }
}