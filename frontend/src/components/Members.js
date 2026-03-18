import { useState, useEffect } from "react";
import { membersApi, trainingPlansApi } from "../services/api";
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, ResponsiveContainer, Tooltip } from "recharts";

const tooltipStyle = { backgroundColor: "#18181c", border: "1px solid #2a2a30", borderRadius: "10px", color: "#eeeef0", fontSize: "13px", padding: "8px 12px" };

function Members() {
  const [members, setMembers] = useState([]);
  const [plans, setPlans] = useState([]);
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [joinDate, setJoinDate] = useState("");
  const [editingId, setEditingId] = useState(null);

  useEffect(() => { load(); }, []);

  async function load() {
    membersApi.getAll().then(setMembers).catch(() => {});
    trainingPlansApi.getAll().then(setPlans).catch(() => {});
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const data = { firstName, lastName, email, joinDate };
    if (editingId) {
      await membersApi.update(editingId, { id: editingId, ...data });
      setEditingId(null);
    } else {
      await membersApi.create(data);
    }
    setFirstName(""); setLastName(""); setEmail(""); setJoinDate("");
    load();
  }

  function handleEdit(m) {
    setEditingId(m.id); setFirstName(m.firstName); setLastName(m.lastName);
    setEmail(m.email); setJoinDate(m.joinDate.split("T")[0]);
  }

  function handleCancelEdit() {
    setEditingId(null); setFirstName(""); setLastName(""); setEmail(""); setJoinDate("");
  }

  async function handleDelete(id) { await membersApi.delete(id); load(); }

  const activePlans = plans.filter((p) => new Date(p.endDate) >= new Date()).length;
  const mostActive = members.reduce((best, m) => {
    const c = plans.filter((p) => p.memberId === m.id).length;
    return c > (best.count || 0) ? { name: `${m.firstName} ${m.lastName}`, count: c } : best;
  }, { name: "—", count: 0 });
  const newest = members.length > 0 ? members[members.length - 1] : null;

  const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
  const joinChart = months.map((m, i) => ({
    month: m,
    members: members.filter((mb) => new Date(mb.joinDate).getMonth() === i).length,
  })).filter((_, i) => i <= new Date().getMonth() + 1);

  return (
    <div className="entity-section">
      <div className="dashboard-stats">
        <div className="stat-card purple">
          <div className="stat-header"><span className="stat-label">Total Members</span><div className="stat-icon-wrap purple">👥</div></div>
          <div className="stat-value">{members.length}</div>
          <div className="stat-footer">Registered members</div>
        </div>
        <div className="stat-card green">
          <div className="stat-header"><span className="stat-label">Active Plans</span><div className="stat-icon-wrap green">✅</div></div>
          <div className="stat-value">{activePlans}</div>
          <div className="stat-footer">Currently active</div>
        </div>
        <div className="stat-card blue">
          <div className="stat-header"><span className="stat-label">Total Plans</span><div className="stat-icon-wrap blue">📋</div></div>
          <div className="stat-value">{plans.length}</div>
          <div className="stat-footer">All time assigned</div>
        </div>
        <div className="stat-card orange">
          <div className="stat-header"><span className="stat-label">Most Active</span><div className="stat-icon-wrap orange">🌟</div></div>
          <div className="stat-value" style={{ fontSize: "20px" }}>{mostActive.name}</div>
          <div className="stat-footer"><span className="stat-trend">{mostActive.count}</span> plans</div>
        </div>
      </div>

      <div className="page-grid">
        <div className="page-grid-main">
          <div className="section-header"><span className="section-title">Manage Members</span><span className="record-count">{members.length} records</span></div>
          <form className="entity-form" onSubmit={handleSubmit}>
            <div className="form-group"><label className="form-label">First Name</label><input className="form-input" placeholder="Jane" value={firstName} onChange={(e) => setFirstName(e.target.value)} required /></div>
            <div className="form-group"><label className="form-label">Last Name</label><input className="form-input" placeholder="Smith" value={lastName} onChange={(e) => setLastName(e.target.value)} required /></div>
            <div className="form-group"><label className="form-label">Email</label><input className="form-input" type="email" placeholder="jane@example.com" value={email} onChange={(e) => setEmail(e.target.value)} required /></div>
            <div className="form-group"><label className="form-label">Join Date</label><input className="form-input" type="date" value={joinDate} onChange={(e) => setJoinDate(e.target.value)} required /></div>
            <div className="form-actions">
              <button type="submit" className="btn btn-primary">{editingId ? "✓ Update" : "+ Add"}</button>
              {editingId && <button type="button" className="btn btn-ghost" onClick={handleCancelEdit}>Cancel</button>}
            </div>
          </form>
          {members.length === 0 ? (
            <div className="table-wrapper"><div className="empty-state"><div className="empty-icon">👥</div><div className="empty-text">No members yet</div><div className="empty-subtext">Add your first member above</div></div></div>
          ) : (
            <div className="table-wrapper">
              <table className="data-table">
                <thead><tr><th>ID</th><th>Member</th><th>Email</th><th>Join Date</th><th>Plans</th><th>Actions</th></tr></thead>
                <tbody>
                  {members.map((m) => {
                    const pc = plans.filter((p) => p.memberId === m.id).length;
                    return (
                      <tr key={m.id}>
                        <td><span className="cell-id">#{m.id}</span></td>
                        <td><div className="cell-user"><div className="cell-avatar">{m.firstName[0]}{m.lastName[0]}</div><div><div className="cell-name">{m.firstName} {m.lastName}</div><div className="cell-sub">Member</div></div></div></td>
                        <td><span className="cell-email">{m.email}</span></td>
                        <td>{new Date(m.joinDate).toLocaleDateString()}</td>
                        <td><span className="cell-badge green">{pc} plans</span></td>
                        <td><div className="cell-actions"><button className="btn-icon" onClick={() => handleEdit(m)} title="Edit">✏️</button><button className="btn-icon danger" onClick={() => handleDelete(m.id)} title="Delete">🗑️</button></div></td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
        <div className="page-grid-side">
          <div className="side-card">
            <div className="chart-title"><div className="chart-title-dot purple"></div> Join Timeline</div>
            <ResponsiveContainer width="100%" height={180}>
              <AreaChart data={joinChart}>
                <defs><linearGradient id="joinGrad" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stopColor="#6c5ce7" stopOpacity={0.3} /><stop offset="100%" stopColor="#6c5ce7" stopOpacity={0} /></linearGradient></defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#1a1a1f" /><XAxis dataKey="month" tick={{ fill: "#505068", fontSize: 11 }} axisLine={false} tickLine={false} /><YAxis tick={{ fill: "#505068", fontSize: 11 }} axisLine={false} tickLine={false} /><Tooltip contentStyle={tooltipStyle} /><Area type="monotone" dataKey="members" stroke="#6c5ce7" strokeWidth={2} fill="url(#joinGrad)" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
          <div className="side-card">
            <div className="chart-title"><div className="chart-title-dot green"></div> Quick Info</div>
            <div className="info-list">
              <div className="info-row"><span className="info-icon">🆕</span><span className="info-label">Newest</span><span className="info-value">{newest ? `${newest.firstName} ${newest.lastName}` : "—"}</span></div>
              <div className="info-row"><span className="info-icon">📧</span><span className="info-label">Emails</span><span className="info-value">{members.length} registered</span></div>
              <div className="info-row"><span className="info-icon">📋</span><span className="info-label">Avg Plans</span><span className="info-value">{members.length > 0 ? (plans.length / members.length).toFixed(1) : "0"}</span></div>
              <div className="info-row"><span className="info-icon">🌟</span><span className="info-label">Most Active</span><span className="info-value">{mostActive.name}</span></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Members;
