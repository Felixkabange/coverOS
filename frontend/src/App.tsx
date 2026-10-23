import { useEffect, useState, type FormEvent } from 'react'
import { Building2, LayoutDashboard, LogOut, ShieldCheck } from 'lucide-react'
import { API } from './api'
import { errorMessage, getAccount, registerAgency, signIn, type Session } from './api/auth'
import './App.css'

function App() {
  const [session, setSession] = useState<Session | null>(null)
  const [page, setPage] = useState('Overview')
  const [register, setRegister] = useState(false)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  function signOut() { API.clearAccessToken(); setSession(null); setPage('Overview'); setError(''); setNotice('') }
  useEffect(() => {
    if (!session) return
    const timer = window.setTimeout(() => {
      API.clearAccessToken(); setSession(null); setNotice('Your session expired. Please sign in again.')
    }, Math.max(0, new Date(session.expiresAt).getTime() - Date.now()))
    return () => window.clearTimeout(timer)
  }, [session])
  async function authenticate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setError(''); setNotice('')
    const form = new FormData(event.currentTarget)
    try {
      const next = register
        ? await registerAgency(String(form.get('email')), String(form.get('password')), String(form.get('name')), String(form.get('agencyName')))
        : await signIn(String(form.get('email')), String(form.get('password')))
      API.setAccessToken(next.accessToken); setSession(next); setPage('Overview')
    } catch (err) { setError(errorMessage(err)) }
    finally { setBusy(false) }
  }
  async function refreshAccount() {
    setBusy(true); setError(''); setNotice('')
    try { const account = await getAccount(); setSession(current => current ? { ...current, account } : null); setNotice('Account information is up to date.') }
    catch (err) { setError(errorMessage(err)) }
    finally { setBusy(false) }
  }
  const brand = <div className="brand"><span className="brand-mark"><ShieldCheck size={21} /></span><span>cover<span className="brand-light">OS</span></span></div>
  if (!session) return <main className="auth-layout">
    <section className="auth-story">{brand}<div><span className="eyebrow">BUILT FOR AGENCIES. CONNECTED TO CONTRACTORS.</span><h1>A clearer view of<br />every requirement.</h1><p>Bring your agency, contractors, and insurance reviews together in one workspace.</p><div className="story-note"><ShieldCheck /><span>From contract to clarity.<br /><small>Agency expertise stays at the heart of every review.</small></span></div></div><small>CoverOS · Insurance operations</small></section>
    <section className="auth-form-wrap"><form className="auth-form" onSubmit={authenticate}>
      <span className="eyebrow">YOUR AGENCY WORKSPACE</span><h1>{register ? 'Create your agency' : 'Welcome back'}</h1><p className="page-subtitle">{register ? 'Start with your account and agency details.' : 'Sign in to your CoverOS account.'}</p>
      {register && <><label>Your name<input name="name" autoComplete="name" required minLength={2} maxLength={120} /></label><label>Agency name<input name="agencyName" autoComplete="organization" required minLength={2} maxLength={160} /></label></>}
      <label>Email address<input name="email" type="email" autoComplete="email" required maxLength={254} /></label>
      <label>Password<input name="password" type="password" autoComplete={register ? 'new-password' : 'current-password'} required minLength={register ? 12 : 1} maxLength={128} /></label>
      {register && <small>Use at least 12 characters. Contractor accounts will join by invitation in the next release.</small>}
      {error && <p className="form-error" role="alert">{error}</p>}{notice && <p role="status">{notice}</p>}
      <button className="button-primary" disabled={busy}>{busy ? 'Please wait…' : register ? 'Create agency & continue' : 'Sign in'}</button>
      <button className="text-button" type="button" disabled={busy} onClick={() => { setRegister(!register); setError('') }}>{register ? 'Already have an account? Sign in' : 'New to CoverOS? Create an agency'}</button>
      <small>Sessions last 30 minutes. Refreshing this page requires a new sign-in.</small>
    </form></section>
  </main>
  const { account } = session
  return <div className="app-shell">
    <aside className="sidebar">{brand}<div className="workspace-switcher"><Building2 size={22} /><div className="workspace-copy"><strong>{account.organizationName}</strong><span>{account.organizationType} workspace</span></div></div><div className="nav-label">WORKSPACE</div><nav className="primary-nav" aria-label="Main navigation">{[{ name: 'Overview', icon: LayoutDashboard }, { name: 'Company', icon: Building2 }].map(({ name, icon: Icon }) => <button title={name} key={name} className={`nav-item ${page === name ? 'is-active' : ''}`} onClick={() => { setPage(name); setNotice(''); setError('') }}><Icon size={18} /><span>{name}</span></button>)}</nav><div className="sidebar-bottom"><p>{account.name}</p><button className="nav-item" title="Sign out" onClick={signOut}><LogOut size={18} /><span>Sign out</span></button></div></aside>
    <main className="main-content"><header className="topbar"><div className="breadcrumbs">Workspace / <strong>{page}</strong></div><span className="role-badge">{account.role === 'AgencyAdmin' ? 'Agency administrator' : account.role}</span></header><div className="page-content"><div className="eyebrow">INSURANCE OPERATIONS</div><h1>{page === 'Overview' ? `Welcome, ${account.name.split(' ')[0]}` : 'Company & account'}</h1><p className="page-subtitle">{page === 'Overview' ? 'Your agency workspace starts here.' : 'Your current organization and membership details.'}</p>
      {error && <p className="form-error" role="alert">{error}</p>}{notice && <p className="notice" role="status">{notice}</p>}
      {page === 'Overview' ? <><section className="welcome-panel panel"><span className="setup-icon"><Building2 size={28} /></span><h2>{account.organizationName} is ready</h2><p>Your agency and administrator account are saved in PostgreSQL. You can sign out and sign back in to test your account.</p><button className="button-primary" onClick={() => setPage('Company')}>View company details</button></section><section className="panel roadmap"><h2>Coming in the next step</h2><p>Invite your first contractor, assign an account manager, and connect their portal.</p><p className="page-subtitle">Projects, document uploads, analysis, and staff approval will follow. Reports will require agency approval before contractors can view them.</p></section></> : <section className="panel account-panel"><h2>Company profile</h2><dl>{[['Organization', account.organizationName], ['Organization type', account.organizationType], ['Your name', account.name], ['Email', account.email], ['Role', account.role], ['Organization ID', account.organizationId]].map(([label, value]) => <div key={label}><dt>{label}</dt><dd>{value}</dd></div>)}</dl><button className="button-secondary" onClick={refreshAccount} disabled={busy}>{busy ? 'Refreshing…' : 'Refresh from database'}</button></section>}
      <footer className="page-footer">CoverOS · Agency workspace · Step 1</footer></div></main>
  </div>
}
export default App
