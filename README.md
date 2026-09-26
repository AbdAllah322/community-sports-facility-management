# Community Sports Facility Management

## Scope
I implemented the nine core requirements in a single unmanaged Dataverse solution with a dedicated publisher and custom prefix. The solution includes the Facility and Booking data model, booking validation and pricing logic, sales hand-off automation, booking form behaviour, maintenance handling, queue routing, and location-based security. Managed and unmanaged solution exports are provided.

Optional extras were not included; I prioritised completing and validating the core requirements.

> Source-code note: the strong-name signing key is intentionally not committed to this public repository. Generate a local `key.snk` and enable assembly signing before registering the plug-in assembly in Dataverse.

## Implementation Approach
1. Solution Packaging — Configuration: All transportable components are contained in one unmanaged solution with a dedicated publisher/custom prefix and exported as both managed and unmanaged packages.
2. Data Model — Dataverse configuration: Facility and Booking are custom tables. Contact is reused for Members. Facility Type is a reusable global Choice. Facility Code is an Alternate Key so external systems can address a Facility without its Dataverse GUID.
3. Prevent Double Booking — Synchronous PreValidation plug-in: The rule must apply to forms, imports and APIs. The plug-in checks the same Facility and overlapping time range, excluding Cancelled bookings, and returns a clear validation message.
4. Booking Pricing — Synchronous PreOperation plug-in: Total Price is system-controlled and calculated in the same transaction from duration × Facility hourly rate. Active members receive a configurable discount stored in an Environment Variable, and price is recalculated when relevant Booking data changes.
5. Sales Hand-off — Power Automate: When an Opportunity becomes Won, a Draft Booking is created and linked to the Opportunity. The Opportunity Contact is copied to Booking Member. This is follow-up automation and does not need to block the Opportunity transaction.
6. Booking Form Behaviour — Configuration + server validation: Facility hourly rate is shown on the Booking form from the selected Facility. Invalid Start/End dates are enforced by a synchronous PreValidation plug-in so the rule also applies outside the UI. Cancellation Reason is controlled by a Business Rule: hidden normally and visible/required when Status Reason is Cancelled.
7. Maintenance — Case + Queue + Power Automate + plug-in: Case is linked to Facility and routed to a Maintenance Queue. On maintenance Case creation, Power Automate sets the Facility to Inactive / Under Maintenance. On Case resolution another flow returns it to Active / Available. A synchronous Booking plug-in prevents bookings against an Under Maintenance Facility.
8. Security — Business Units + Security Roles: Business Units represent locations. Booking is User/Team-owned. Coordinators receive Business Unit-level Booking access and no Facility Delete privilege. Managers receive Organization-level Booking Read so they can view bookings across locations.
9. README — Documentation: This file records the implementation choices, assumptions, limitations and rationale.

## Assumptions
- The brief explicitly defines Draft and Cancelled Booking statuses; Booked was added as the normal confirmed-booking state.
- Membership structure and discount percentage were not defined. Contact therefore has an Active Membership Yes/No field, and all active members receive one configurable discount percentage from an Environment Variable.
- The Sales hand-off requirement does not define Facility or booking dates on Opportunity. The flow therefore copies the Opportunity Contact to Booking Member but leaves Facility, Start Time and End Time for the Coordinator to complete.
- The brief does not define a Location table. Locations are represented by Dataverse Business Units, and the Booking owning Business Unit represents its location.
- Facility is Organization-owned because it is shared reference data, while Booking is User/Team-owned because location-based security is required.
- Business Units, BU-specific security-role instances and user assignments are environment-specific configuration. The child-location security roles and user assignments are therefore configured directly in the assessment environment rather than packaged in the solution.

## With Another Week
I would add automated tests for the server-side booking rules, unpack the solution into source control with a structured commit history, and perform additional concurrency and end-to-end testing.

The main limitation I would investigate further is the short asynchronous window between creating a maintenance Case and the Power Automate flow changing the Facility to Inactive / Under Maintenance. During that brief interval, a booking request could theoretically reach Dataverse while the Facility still appears Available. For this assessment I intentionally kept the maintenance status transition in Power Automate because it is simple, maintainable and appropriate for the required automation. If the production system required zero tolerance for that timing window, I would evaluate moving only the Facility status transition to synchronous server-side logic while keeping queue routing and the remaining maintenance automation in Power Automate.
