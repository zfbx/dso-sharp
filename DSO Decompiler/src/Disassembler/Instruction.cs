﻿using System.Collections.Generic;

namespace DSODecompiler.Disassembler
{
	public class Instruction
	{
		public Opcodes.Ops Op { get; }
		public uint Addr { get; }

		public Instruction Prev { get; set; } = null;
		public Instruction Next { get; set; } = null;

		public Instruction (Opcodes.Ops op, uint addr)
		{
			Op = op;
			Addr = addr;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}]";
		}
	}

	public class FuncDeclInsn : Instruction
	{
		public string Name { get; set; } = null;
		public string Namespace { get; set; } = null;
		public string Package { get; set; } = null;
		public bool HasBody { get; set; }
		public uint EndAddr { get; set; }

		public readonly List<string> Arguments = new();

		public FuncDeclInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}

		public override string ToString ()
		{
			var str = $"[@{Addr}, {GetType().Name}, \"{Name}\", \"{Namespace}\", \"{Package}\", {HasBody}, {EndAddr}";

			foreach (var arg in Arguments)
			{
				str += $", {arg}";
			}

			return $"{str}]";
		}
	}

	public class CreateObjectInsn : Instruction
	{
		public string ParentName { get; set; } = null;
		public bool IsDataBlock { get; set; } = false;
		public uint FailJumpAddr { get; set; }

		public CreateObjectInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, \"{ParentName}\", {IsDataBlock}, {FailJumpAddr}]";
		}
	}

	public class AddObjectInsn : Instruction
	{
		public bool PlaceAtRoot { get; }

		public AddObjectInsn (Opcodes.Ops op, uint addr, bool placeAtRoot) : base(op, addr)
		{
			PlaceAtRoot = placeAtRoot;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, {PlaceAtRoot}]";
		}
	}

	public class EndObjectInsn : Instruction
	{
		public bool Value { get; }

		public EndObjectInsn (Opcodes.Ops op, uint addr, bool value) : base(op, addr)
		{
			Value = value;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, {Value}]";
		}
	}

	public class BranchInsn : Instruction
	{
		public uint TargetAddr { get; }
		public Opcodes.BranchType Type { get; }

		public BranchInsn (Opcodes.Ops op, uint addr, uint target, Opcodes.BranchType type) : base(op, addr)
		{
			TargetAddr = target;
			Type = type;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, {TargetAddr}, {Type}]";
		}
	}

	public class ReturnInsn : Instruction
	{
		public bool ReturnsValue { get; }

		public ReturnInsn (Opcodes.Ops op, uint addr, bool returnsValue) : base(op, addr)
		{
			ReturnsValue = returnsValue;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, {ReturnsValue}]";
		}
	}

	public class BinaryInsn : Instruction
	{
		public BinaryInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class StringCompareInsn : Instruction
	{
		public StringCompareInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class UnaryInsn : Instruction
	{
		public UnaryInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class SetCurVarInsn : Instruction
	{
		public string Name { get; }

		public SetCurVarInsn (Opcodes.Ops op, uint addr, string name) : base(op, addr)
		{
			Name = name;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, \"{Name}\"]";
		}
	}

	public class SetCurVarArrayInsn : Instruction
	{
		public SetCurVarArrayInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class LoadVarInsn : Instruction
	{
		public LoadVarInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class SaveVarInsn : Instruction
	{
		public SaveVarInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class SetCurObjectInsn : Instruction
	{
		public bool IsNew { get; }

		public SetCurObjectInsn (Opcodes.Ops op, uint addr, bool isNew) : base(op, addr)
		{
			IsNew = isNew;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, {IsNew}]";
		}
	}

	public class SetCurFieldInsn : Instruction
	{
		public string Name { get; }

		public SetCurFieldInsn (Opcodes.Ops op, uint addr, string name) : base(op, addr)
		{
			Name = name;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, \"{Name}\"]";
		}
	}

	public class SetCurFieldArrayInsn : Instruction
	{
		public SetCurFieldArrayInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class LoadFieldInsn : Instruction
	{
		public LoadFieldInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class SaveFieldInsn : Instruction
	{
		public SaveFieldInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class ConvertToTypeInsn : Instruction
	{
		public Opcodes.ConvertToType Type { get; }

		public ConvertToTypeInsn (Opcodes.Ops op, uint addr, Opcodes.ConvertToType type) : base(op, addr)
		{
			Type = type;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, {Type}]";
		}
	}

	public class LoadImmedInsn : Instruction
	{
		// Strings and floats rely on knowing whether we're in a function, which we're not going
		// to do until later, so we just store the raw table index.
		public uint Value { get; }

		public LoadImmedInsn (Opcodes.Ops op, uint addr, uint value) : base(op, addr)
		{
			Value = value;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, {Value}]";
		}
	}

	public class FuncCallInsn : Instruction
	{
		public string Name { get; set; } = null;
		public string Namespace { get; set; } = null;
		public uint CallType { get; set; } = 0;

		public FuncCallInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, \"{Name}\", \"{Namespace}\", {CallType}]";
		}
	}

	public class AdvanceStringInsn : Instruction
	{
		public Opcodes.AdvanceStringType Type { get; }
		public char Char { get; }
		public bool HasChar { get; }

		public AdvanceStringInsn (Opcodes.Ops op, uint addr, Opcodes.AdvanceStringType type, char ch)
			: base(op, addr)
		{
			Type = type;
			Char = ch;
			HasChar = true;
		}

		public AdvanceStringInsn (Opcodes.Ops op, uint addr, Opcodes.AdvanceStringType type = Opcodes.AdvanceStringType.Default)
			: base(op, addr)
		{
			Type = type;
			Char = '\0';
			HasChar = false;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, {Type}{(HasChar ? $", {(byte) Char}" : "")}]";
		}
	}

	public class RewindInsn : Instruction
	{
		public bool Terminate { get; }

		public RewindInsn (Opcodes.Ops op, uint addr, bool terminate = false) : base(op, addr)
		{
			Terminate = terminate;
		}

		public override string ToString ()
		{
			return $"[@{Addr}, {GetType().Name}, {Terminate}]";
		}
	}

	public class PushInsn : Instruction
	{
		public PushInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class PushFrameInsn : Instruction
	{
		public PushFrameInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class DebugBreakInsn : Instruction
	{
		public DebugBreakInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}

	public class UnusedInsn : Instruction
	{
		public UnusedInsn (Opcodes.Ops op, uint addr) : base(op, addr) {}
	}
}
