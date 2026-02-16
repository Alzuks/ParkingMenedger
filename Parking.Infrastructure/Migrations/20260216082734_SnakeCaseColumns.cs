using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- passages (PascalCase -> snake_case)
            migrationBuilder.RenameColumn(name: "Id", table: "passages", newName: "id");
            migrationBuilder.RenameColumn(name: "OccurredAt", table: "passages", newName: "occurred_at");
            migrationBuilder.RenameColumn(name: "PlateRaw", table: "passages", newName: "plate_raw");
            migrationBuilder.RenameColumn(name: "PlateNorm", table: "passages", newName: "plate_norm");
            migrationBuilder.RenameColumn(name: "Direction", table: "passages", newName: "direction");
            migrationBuilder.RenameColumn(name: "JpegPath", table: "passages", newName: "jpeg_path");
            // confidence уже нормально: confidence

            // --- parking_sessions (PascalCase -> snake_case)
            migrationBuilder.RenameColumn(name: "Id", table: "parking_sessions", newName: "id");
            migrationBuilder.RenameColumn(name: "PlateNorm", table: "parking_sessions", newName: "plate_norm");
            migrationBuilder.RenameColumn(name: "OpenedAt", table: "parking_sessions", newName: "opened_at");
            migrationBuilder.RenameColumn(name: "ClosedAt", table: "parking_sessions", newName: "closed_at");

            // --- owners
            migrationBuilder.RenameColumn(name: "firstname", table: "owners", newName: "first_name");
            migrationBuilder.RenameColumn(name: "lastname", table: "owners", newName: "last_name");
            migrationBuilder.RenameColumn(name: "isactive", table: "owners", newName: "is_active");

            // --- employees
            migrationBuilder.RenameColumn(name: "firstname", table: "employees", newName: "first_name");
            migrationBuilder.RenameColumn(name: "lastname", table: "employees", newName: "last_name");
            migrationBuilder.RenameColumn(name: "isactive", table: "employees", newName: "is_active");

            // --- vehicles
            migrationBuilder.RenameColumn(name: "platenorm", table: "vehicles", newName: "plate_norm");
            migrationBuilder.RenameColumn(name: "plateraw", table: "vehicles", newName: "plate_raw");
            migrationBuilder.RenameColumn(name: "isactive", table: "vehicles", newName: "is_active");

            // --- vehicle_owners (M:N)
            migrationBuilder.RenameColumn(name: "vehicleid", table: "vehicle_owners", newName: "vehicle_id");
            migrationBuilder.RenameColumn(name: "ownerid", table: "vehicle_owners", newName: "owner_id");
            migrationBuilder.RenameColumn(name: "ispayer", table: "vehicle_owners", newName: "is_payer");

            // --- contract_places
            migrationBuilder.RenameColumn(name: "contractid", table: "contract_places", newName: "contract_id");
            migrationBuilder.RenameColumn(name: "placeid", table: "contract_places", newName: "place_id");
            migrationBuilder.RenameColumn(name: "linkedcontractid", table: "contract_places", newName: "linked_contract_id");
            migrationBuilder.RenameColumn(name: "linkedsince", table: "contract_places", newName: "linked_since");

            // --- contract_vehicles
            migrationBuilder.RenameColumn(name: "contractid", table: "contract_vehicles", newName: "contract_id");
            migrationBuilder.RenameColumn(name: "vehicleid", table: "contract_vehicles", newName: "vehicle_id");

            // --- contracts
            migrationBuilder.RenameColumn(name: "customerownerid", table: "contracts", newName: "customer_owner_id");
            migrationBuilder.RenameColumn(name: "tariffid", table: "contracts", newName: "tariff_id");
            migrationBuilder.RenameColumn(name: "startat", table: "contracts", newName: "start_at");
            migrationBuilder.RenameColumn(name: "paiduntil", table: "contracts", newName: "paid_until");
            migrationBuilder.RenameColumn(name: "pausebalancedays", table: "contracts", newName: "pause_balance_days");
            migrationBuilder.RenameColumn(name: "pausedat", table: "contracts", newName: "paused_at");

            // --- payments
            migrationBuilder.RenameColumn(name: "contractid", table: "payments", newName: "contract_id");
            migrationBuilder.RenameColumn(name: "paidat", table: "payments", newName: "paid_at");
            migrationBuilder.RenameColumn(name: "employeeid", table: "payments", newName: "employee_id");
            migrationBuilder.RenameColumn(name: "receiptno", table: "payments", newName: "receipt_no");
            migrationBuilder.RenameColumn(name: "coveredfrom", table: "payments", newName: "covered_from");
            migrationBuilder.RenameColumn(name: "coveredto", table: "payments", newName: "covered_to");
            migrationBuilder.RenameColumn(name: "evidenceref", table: "payments", newName: "evidence_ref");

            // --- places
            migrationBuilder.RenameColumn(name: "placeno", table: "places", newName: "place_no");
            migrationBuilder.RenameColumn(name: "isactive", table: "places", newName: "is_active");

            // --- tariffs
            migrationBuilder.RenameColumn(name: "billingmodel", table: "tariffs", newName: "billing_model");
            migrationBuilder.RenameColumn(name: "isactive", table: "tariffs", newName: "is_active");

            // --- tariff_rates
            migrationBuilder.RenameColumn(name: "tariffid", table: "tariff_rates", newName: "tariff_id");
            migrationBuilder.RenameColumn(name: "validfrom", table: "tariff_rates", newName: "valid_from");
            migrationBuilder.RenameColumn(name: "validto", table: "tariff_rates", newName: "valid_to");

            // --- watchlist
            migrationBuilder.RenameColumn(name: "platenorm", table: "watchlist", newName: "plate_norm");
            migrationBuilder.RenameColumn(name: "notifyonin", table: "watchlist", newName: "notify_on_in");
            migrationBuilder.RenameColumn(name: "notifyonout", table: "watchlist", newName: "notify_on_out");
            migrationBuilder.RenameColumn(name: "isactive", table: "watchlist", newName: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- passages
            migrationBuilder.RenameColumn("id", "passages", "Id");
            migrationBuilder.RenameColumn("occurred_at", "passages", "OccurredAt");
            migrationBuilder.RenameColumn("plate_raw", "passages", "PlateRaw");
            migrationBuilder.RenameColumn("plate_norm", "passages", "PlateNorm");
            migrationBuilder.RenameColumn("direction", "passages", "Direction");
            migrationBuilder.RenameColumn("jpeg_path", "passages", "JpegPath");

            // --- parking_sessions
            migrationBuilder.RenameColumn("id", "parking_sessions", "Id");
            migrationBuilder.RenameColumn("plate_norm", "parking_sessions", "PlateNorm");
            migrationBuilder.RenameColumn("opened_at", "parking_sessions", "OpenedAt");
            migrationBuilder.RenameColumn("closed_at", "parking_sessions", "ClosedAt");

            // --- owners
            migrationBuilder.RenameColumn("first_name", "owners", "firstname");
            migrationBuilder.RenameColumn("last_name", "owners", "lastname");
            migrationBuilder.RenameColumn("is_active", "owners", "isactive");

            // --- employees
            migrationBuilder.RenameColumn("first_name", "employees", "firstname");
            migrationBuilder.RenameColumn("last_name", "employees", "lastname");
            migrationBuilder.RenameColumn("is_active", "employees", "isactive");

            // --- vehicles
            migrationBuilder.RenameColumn("plate_norm", "vehicles", "platenorm");
            migrationBuilder.RenameColumn("plate_raw", "vehicles", "plateraw");
            migrationBuilder.RenameColumn("is_active", "vehicles", "isactive");

            // --- vehicle_owners
            migrationBuilder.RenameColumn("vehicle_id", "vehicle_owners", "vehicleid");
            migrationBuilder.RenameColumn("owner_id", "vehicle_owners", "ownerid");
            migrationBuilder.RenameColumn("is_payer", "vehicle_owners", "ispayer");

            // --- contract_places
            migrationBuilder.RenameColumn("contract_id", "contract_places", "contractid");
            migrationBuilder.RenameColumn("place_id", "contract_places", "placeid");
            migrationBuilder.RenameColumn("linked_contract_id", "contract_places", "linkedcontractid");
            migrationBuilder.RenameColumn("linked_since", "contract_places", "linkedsince");

            // --- contract_vehicles
            migrationBuilder.RenameColumn("contract_id", "contract_vehicles", "contractid");
            migrationBuilder.RenameColumn("vehicle_id", "contract_vehicles", "vehicleid");

            // --- contracts
            migrationBuilder.RenameColumn("customer_owner_id", "contracts", "customerownerid");
            migrationBuilder.RenameColumn("tariff_id", "contracts", "tariffid");
            migrationBuilder.RenameColumn("start_at", "contracts", "startat");
            migrationBuilder.RenameColumn("paid_until", "contracts", "paiduntil");
            migrationBuilder.RenameColumn("pause_balance_days", "contracts", "pausebalancedays");
            migrationBuilder.RenameColumn("paused_at", "contracts", "pausedat");

            // --- payments
            migrationBuilder.RenameColumn("contract_id", "payments", "contractid");
            migrationBuilder.RenameColumn("paid_at", "payments", "paidat");
            migrationBuilder.RenameColumn("employee_id", "payments", "employeeid");
            migrationBuilder.RenameColumn("receipt_no", "payments", "receiptno");
            migrationBuilder.RenameColumn("covered_from", "payments", "coveredfrom");
            migrationBuilder.RenameColumn("covered_to", "payments", "coveredto");
            migrationBuilder.RenameColumn("evidence_ref", "payments", "evidenceref");

            // --- places
            migrationBuilder.RenameColumn("place_no", "places", "placeno");
            migrationBuilder.RenameColumn("is_active", "places", "isactive");

            // --- tariffs
            migrationBuilder.RenameColumn("billing_model", "tariffs", "billingmodel");
            migrationBuilder.RenameColumn("is_active", "tariffs", "isactive");

            // --- tariff_rates
            migrationBuilder.RenameColumn("tariff_id", "tariff_rates", "tariffid");
            migrationBuilder.RenameColumn("valid_from", "tariff_rates", "validfrom");
            migrationBuilder.RenameColumn("valid_to", "tariff_rates", "validto");

            // --- watchlist
            migrationBuilder.RenameColumn("plate_norm", "watchlist", "platenorm");
            migrationBuilder.RenameColumn("notify_on_in", "watchlist", "notifyonin");
            migrationBuilder.RenameColumn("notify_on_out", "watchlist", "notifyonout");
            migrationBuilder.RenameColumn("is_active", "watchlist", "isactive");
        }
    }
}
