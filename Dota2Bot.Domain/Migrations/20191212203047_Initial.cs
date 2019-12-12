using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Dota2Bot.Domain.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "heroes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_heroes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    steamid = table.Column<string>(nullable: true),
                    solo_rank = table.Column<int>(nullable: false),
                    party_rank = table.Column<int>(nullable: false),
                    win_count = table.Column<int>(nullable: false),
                    lose_count = table.Column<int>(nullable: false),
                    last_match_id = table.Column<long>(nullable: true),
                    last_match_date = table.Column<DateTime>(nullable: true),
                    last_rating_date = table.Column<DateTime>(nullable: true),
                    rank_tier = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tg_chats",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false),
                    timezone = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tg_chats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "matches",
                schema: "public",
                columns: table => new
                {
                    match_id = table.Column<long>(nullable: false),
                    player_id = table.Column<long>(nullable: false),
                    player_slot = table.Column<short>(nullable: false),
                    hero_id = table.Column<int>(nullable: false),
                    kills = table.Column<int>(nullable: false),
                    deaths = table.Column<int>(nullable: false),
                    assists = table.Column<int>(nullable: false),
                    date_start = table.Column<DateTime>(nullable: false),
                    duration = table.Column<TimeSpan>(nullable: false),
                    game_mode = table.Column<int>(nullable: false),
                    lobby_type = table.Column<int>(nullable: false),
                    won = table.Column<bool>(nullable: false),
                    leaver_status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => new { x.match_id, x.player_id });
                    table.ForeignKey(
                        name: "FK_matches_heroes_hero_id",
                        column: x => x.hero_id,
                        principalSchema: "public",
                        principalTable: "heroes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_matches_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "public",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rating",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<long>(nullable: false),
                    match_id = table.Column<long>(nullable: false),
                    solo_rank = table.Column<int>(nullable: false),
                    party_rank = table.Column<int>(nullable: false),
                    date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rating", x => x.id);
                    table.ForeignKey(
                        name: "FK_rating_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "public",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tg_chat_players",
                schema: "public",
                columns: table => new
                {
                    id_chat = table.Column<long>(nullable: false),
                    id_player = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tg_chat_players", x => new { x.id_chat, x.id_player });
                    table.ForeignKey(
                        name: "FK_tg_chat_players_tg_chats_id_chat",
                        column: x => x.id_chat,
                        principalSchema: "public",
                        principalTable: "tg_chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tg_chat_players_players_id_player",
                        column: x => x.id_player,
                        principalSchema: "public",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_matches_hero_id",
                schema: "public",
                table: "matches",
                column: "hero_id");

            migrationBuilder.CreateIndex(
                name: "IX_matches_player_id",
                schema: "public",
                table: "matches",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_rating_player_id",
                schema: "public",
                table: "rating",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_tg_chat_players_id_player",
                schema: "public",
                table: "tg_chat_players",
                column: "id_player");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matches",
                schema: "public");

            migrationBuilder.DropTable(
                name: "rating",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tg_chat_players",
                schema: "public");

            migrationBuilder.DropTable(
                name: "heroes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tg_chats",
                schema: "public");

            migrationBuilder.DropTable(
                name: "players",
                schema: "public");
        }
    }
}
