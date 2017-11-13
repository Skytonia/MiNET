using System;
using System.Numerics;
using MiNET.Utils;
using MiNET.Worlds;

namespace MiNET.Blocks
{
	public class Bed : Block
	{

		public const byte FlagOccupied = 0x04;
		public const byte FlagHead = 0x08;

		public Bed() : base(26)
		{
			BlastResistance = 1;
			Hardness = 0.2f;
			IsTransparent = true;
			//IsFlammable = true; // It can catch fire from lava, but not other means.
		}

		public override void BreakBlock(Level level, bool silent = false)
		{
			BlockCoordinates direction = new BlockCoordinates();
			switch (Metadata & 0x07)
			{
				case 0:
					direction = Level.East;
					break; // West
				case 1:
					direction = Level.South;
					break; // South
				case 2:
					direction = Level.West;
					break; // East
				case 3:
					direction = Level.North;
					break; // North 
			}

			// Remove bed
			if ((Metadata & 0x08) != 0x08)
			{
				direction = direction*-1;
			}

			level.SetAir(Coordinates + direction);
		    level.SetAir(Coordinates);
		}

		public override bool Interact(Level world, Player player, BlockCoordinates blockCoordinates, BlockFace face, Vector3 faceCoord)
		{
			Bed headSide = this,
				footSide;
			if (!headSide.IsHead())
			{
				headSide = GetOtherSide(world);
				footSide = this;
			}
			else
			{
				footSide = GetOtherSide(world);
			}

			if (headSide == null || footSide == null)
			{
				player.SendMessage(ChatColors.Gray + "This bed is incomplete");
				return true;
			}

			BlockCoordinates playerCoordinates = player.KnownPosition.GetCoordinates3D();
			if (playerCoordinates.DistanceTo(headSide.Coordinates) > 4 &&
			    playerCoordinates.DistanceTo(footSide.Coordinates) > 4)
			{
				//No message
				return true;
			}

			if (headSide.IsOccupied())
			{
				player.SendMessage(ChatColors.Gray + "%tile.bed.occupied", MessageType.Translation);
				return true;
			}

			player.SleepOn(headSide.Coordinates);

			Console.WriteLine($"Attempting to sleep at ({headSide.Coordinates})");

			return true;
		}

		public bool IsHead()
		{
			return (Metadata & FlagHead) == FlagHead;
		}

		public bool IsOccupied()
		{
			return (Metadata & FlagOccupied) == FlagOccupied;
		}

		public void SetOccupied(Level level, bool occupied)
		{
			Metadata ^= FlagOccupied;
			Console.WriteLine($"Setting at {Coordinates} to {occupied} ({IsOccupied()})");

			level.SetBlock(this);

			Bed otherSide = GetOtherSide(level);

			if(otherSide != null && otherSide.IsOccupied() != occupied)
			{
				otherSide.SetOccupied(level, occupied);
			}
		}

		public Bed GetOtherSide(Level level)
		{
			BlockCoordinates direction = new BlockCoordinates();
			switch (Metadata & 0x07)
			{
				case 0:
					direction = Level.East;
					break; // West
				case 1:
					direction = Level.South;
					break; // South
				case 2:
					direction = Level.West;
					break; // East
				case 3:
					direction = Level.North;
					break; // North 
			}

			if (!IsHead())
			{
				direction *= -1;
			}

			Console.WriteLine($"Block at {Coordinates + direction} is {level.GetBlock(Coordinates + direction).GetType()}");

			return level.GetBlock(Coordinates + direction) as Bed;
		}

		public override BoundingBox GetBoundingBox()
		{
			Vector3 basePosition = new Vector3(Coordinates.X, Coordinates.Y, Coordinates.Z);
			return new BoundingBox(basePosition, basePosition + new Vector3(1f, 0.5625f, 1f));
		}
	}
}