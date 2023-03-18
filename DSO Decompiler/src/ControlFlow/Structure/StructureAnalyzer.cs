﻿using System;
using System.Collections.Generic;

using DSODecompiler.ControlFlow.Structure.Regions;
using DSODecompiler.Disassembler;

namespace DSODecompiler.ControlFlow.Structure
{
	public class StructureAnalyzer
	{
		protected Disassembly disassembly = null;
		protected RegionGraph regionGraph = null;
		protected DominatorGraph<uint, RegionGraphNode> domGraph = null;
		protected Dictionary<uint, VirtualRegion> virtualRegions = null;

		public VirtualRegion Analyze (ControlFlowGraph cfg, Disassembly disasm)
		{
			disassembly = disasm;
			regionGraph = RegionGraph.From(cfg);
			domGraph = new(regionGraph);
			virtualRegions = new();

			var entryAddr = regionGraph.EntryPoint.Addr;

			while (regionGraph.Count > 1)
			{
				foreach (var node in regionGraph.PostorderDFS())
				{
					if (node.Successors.Count <= 0)
					{
						continue;
					}

					var reduced = true;

					while (reduced)
					{
						if (node.Successors.Count > 2)
						{
							throw new NotImplementedException($"Region graph node with more than 2 successors");
						}

						reduced = ReduceAcyclic(node);

						/*if (!reduced && IsCyclic(node))
						{
							reduced = ReduceCyclic(node);
						}*/
					}
				}
			}

			return GetVirtualRegion(entryAddr);
		}

		/*protected bool IsCyclic (RegionGraphNode node)
		{
			return node.Predecessors.Any(predecessor => predecessor == node || IsBackEdge(predecessor, node));
		}

		protected bool IsBackEdge (RegionGraphNode node1, RegionGraphNode node2)
		{
			return domGraph.Dominates(node2, node1, strictly: true);
		}

		// TODO: Test expression inversion and comparison.
		protected bool ReduceCyclic (RegionGraphNode node)
		{
			if (!node.IsLoopStart)
			{
				return false;
			}

			var region = node.Region;

			if (region.LastInstruction is not BranchInstruction)
			{
				throw new NotImplementedException($"Cyclic region at {node.Addr} does not end with branch instruction.");
			}

			foreach (var successor in node.Successors.ToArray())
			{
				if (successor == node)
				{
					var container = new ContainerRegion(successor.Addr);

					region.VirtualRegions.ForEach(vr => container.Children.Add(vr));
					region.VirtualRegions.Clear();
					region.VirtualRegions.Add(container);

					regionGraph.RemoveEdge(node, successor);
					regionGraph.RemoveEdge(successor, node);

					return true;
				}
			}

			foreach (var successor in node.Successors.ToArray())
			{
				if (successor.FirstSuccessor == node && successor.Predecessors.Count == 1)
				{
					var succRegion = successor.Region;

					if (succRegion.LastInstruction is not BranchInstruction branch)
					{
						throw new NotImplementedException($"Cyclic region at {node.Addr} does not end with branch instruction.");
					}

					var container = new ContainerRegion(successor.Addr);

					region.VirtualRegions.ForEach(vr => container.Children.Add(vr));
					succRegion.VirtualRegions.ForEach(vr => container.Children.Add(vr));

					region.VirtualRegions.Clear();
					region.VirtualRegions.Add(container);

					regionGraph.RemoveEdge(node, successor);
					regionGraph.RemoveEdge(successor, node);

					return true;
				}
			}

			return false;
		}*/

		protected bool ReduceAcyclic (RegionGraphNode node)
		{
			// TODO: Acyclic nodes that jump backwards.

			var reduced = false;

			switch (node.Successors.Count)
			{
				case 0:
				{
					break;
				}

				case 1:
				{
					reduced = ReduceSequence(node);
					break;
				}

				case 2:
				{
					reduced = ReduceConditional(node);
					break;
				}

				default:
				{
					throw new NotImplementedException($"Region graph node has {node.Successors.Count} successors");
				}
			}

			return reduced;
		}

		// TODO: Test expression inversion and comparison.
		protected bool ReduceConditional (RegionGraphNode node)
		{
			var region = node.Region;
			var successors = regionGraph.GetSuccessors(node);

			if (successors.Count != 2)
			{
				return false;
			}

			var then = successors[0];
			var @else = successors[1];
			var thenSucc = regionGraph.FirstSuccessor(then);
			var elseSucc = regionGraph.FirstSuccessor(@else);

			var reduced = false;

			// This should never happen.
			if (elseSucc == then)
			{
				throw new Exception($"Unexpected conditional inversion at {node.Addr}");
			}
			else if (thenSucc == @else)
			{
				reduced = true;

				Console.WriteLine($"{node.Addr} :: if-then");

				if (!HasVirtualRegion(then.Addr))
				{
					AddVirtualRegion(then.Addr, new SequenceRegion(then.Region));
				}

				AddVirtualRegion(node.Addr, new ConditionalRegion(node.Region, GetVirtualRegion(then)));

				regionGraph.RemoveEdge(node, then);
				regionGraph.RemoveEdge(then, thenSucc);
				regionGraph.Remove(then);
			}
			else if (elseSucc != null && thenSucc == elseSucc)
			{
				reduced = true;

				Console.WriteLine($"{node.Addr} :: if-then-else");

				if (!HasVirtualRegion(then.Addr))
				{
					AddVirtualRegion(then.Addr, new SequenceRegion(then.Region));
				}

				if (!HasVirtualRegion(@else.Addr))
				{
					AddVirtualRegion(@else.Addr, new SequenceRegion(@else.Region));
				}

				AddVirtualRegion(
					node.Addr,
					new ConditionalRegion(
						node.Region,
						GetVirtualRegion(then),
						GetVirtualRegion(@else)
					)
				);

				regionGraph.RemoveEdge(then, thenSucc);
				regionGraph.RemoveEdge(@else, elseSucc);
				regionGraph.RemoveEdge(node, then);
				regionGraph.RemoveEdge(node, @else);

				regionGraph.AddEdge(node, elseSucc);

				regionGraph.Remove(then);
				regionGraph.Remove(@else);
			}
			else
			{
				Console.WriteLine($"{node.Addr} :: <failed>    {node.Instructions[^1]}");
			}

			return reduced;
		}

		protected bool ReduceSequence (RegionGraphNode node)
		{
			if (node.Successors.Count != 1)
			{
				return false;
			}

			var next = regionGraph.FirstSuccessor(node);

			// Don't want to accidentally delete a jump target.
			if (next.Predecessors.Count > 1)
			{
				return false;
			}

			var sequence = new SequenceRegion();

			if (HasVirtualRegion(node.Addr))
			{
				sequence.Add(GetVirtualRegion(node));
			}

			if (!HasVirtualRegion(next.Addr))
			{
				AddVirtualRegion(next.Addr, new SequenceRegion(next.Region));
			}

			sequence.Add(GetVirtualRegion(next));

			AddVirtualRegion(node.Addr, sequence);

			regionGraph.RemoveEdge(node, next);
			regionGraph.ReplaceSuccessors(next, node);
			regionGraph.Remove(next);

			return true;
		}

		protected VirtualRegion AddVirtualRegion (uint addr, VirtualRegion vr) => virtualRegions[addr] = vr;
		protected bool HasVirtualRegion (uint addr) => virtualRegions.ContainsKey(addr);
		protected VirtualRegion GetVirtualRegion (uint addr) => HasVirtualRegion(addr) ? virtualRegions[addr] : null;
		protected VirtualRegion GetVirtualRegion (RegionGraphNode node) => GetVirtualRegion(node.Addr);
	}
}
