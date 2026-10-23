# CoverOS implementation checkpoints

Source: CoverOS_Database_Design.docx and CoverOS_App_Workflow_and_Features.docx supplied by the owner. These are product specifications; repository instructions and the owner's explicit requests govern implementation.

1. **Current checkpoint — agency identity and database foundation.** Agency registration, hashed passwords, JWT login, active membership lookup, Overview, read-only Company page, and PostgreSQL migrations. Public registration only creates an agency administrator. Contractor and staff accounts must be invitation based.
2. **After owner testing — contractor onboarding.** Contractor directory, agency–contractor links, assigned staff, hashed expiring one-use invitation tokens, email-matched acceptance, contractor login and profile. Validate organization type and assigned staff membership when creating links; never authorize through link IDs supplied by the client alone.
3. **Projects and documents.** Organization-scoped projects, private PDF storage, metadata/versioning, authorized short-lived downloads and audit events.
4. **Analysis and human review.** Background jobs, retries, requirements, policy evidence, findings with source pages and model versions, manual corrections and review tasks.
5. **Published contractor reports.** Explicit publication gate, approved reports only, clarification requests, notifications and activity history.

## PostgreSQL schema rollout

The initial migration creates `users`, `organizations`, `organization_members`, and `agency_contractors`, with UUID keys, unique normalized emails, membership/link uniqueness, foreign keys with restricted deletes, and UTC account/company creation timestamps. One user can have multiple memberships. Login currently selects one active membership deterministically; a membership switcher belongs with multi-organization onboarding.

Future migrations add `invitations`, `projects`, `documents`, `analyses`, `requirements`, `policy_evidence`, `findings`, `reviews`, `tasks`, `notifications`, and `audit_events` as their workflows are implemented and tested. No AI findings may be exposed to contractors before staff approval. Agency access to contractor resources must check both active membership and an active agency–contractor link.

## First checkpoint acceptance test

- Start PostgreSQL, apply migrations, start API and frontend using README.md.
- Create an agency with your name, agency name, email and a password of at least 12 characters.
- Verify Overview and Company show your real account details.
- Refresh company details from the database.
- Sign out. A wrong password must fail; your original password must sign you in.
- Try registering the same email in different letter casing; registration must fail.
- Reload the browser. Sign-in is required again because tokens live only in memory.
- Check that anonymous `GET /api/auth/me` returns 401.

This checkpoint does not yet include email verification, password recovery, refresh tokens, invitations, contractor provisioning, file uploads or analysis. Authentication rate limiting is per API process and assumes a direct connection; configure trusted proxies and a distributed limiter before scaling behind a proxy. Use HTTPS outside local development.
