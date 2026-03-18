import { useState, useEffect } from "react";
import { trainingPlansApi, membersApi, trainingsApi } from "../services/api";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, ResponsiveContainer, Tooltip } from "recharts";

const tooltipStyle = { backgroundColor: "#18181c", border: "1px solid #2a2a30", borderRadius: "10px", color: "#eeeef0", fontSize: "13px", padding: "8px 12px" };

function TrainingPlans() {
  const [plans, setPlans] = useState([]);
  const [members, setMembers] = useState([]);
  const [trainings, setTrainings] = useState([]);
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [memberId, setMemberId] = useState("");
  const [trainingId, setTrainingId] = useState("");
  const [editingId, setEditingId] = useState(null);

  useEffect(() => { load(); }, []);

  async function load() {
    trainingPlansApi.getAll().then(setPlans).catch(() => {});
    membersApi.getAll().then(setMembers).catch(() => {});
    trainingsApi.getAll().then(setTrainings).catch(() => {});
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const data = { startDate, endDate, memberId: parseInt(memberId), trainingId: parseInt(trainingId) };
    if (editingId) {
      await trainingPlansApi.update(editingId, { id: editingId, ...data });
      setEditingId(null);
    } else {
      await trainingPlansApi.create(data);
    }
    setStartDate(""); setEndDate(""); setMemberId(""); setTrainingId("");
    load();
  }

  function handleEdit(p) {
    setEditingId(p.id); setStartDate(p.startDate.split("T")[0]); setEndDate(p.endDate.split("T")[0]);
    setMemberId(p.memberId.toString()); setTrainingId(p.trainingId.toString());
  }

  function handleCancelEdit() {
    setEditingId(null); setStartDate(""); setEndDate(""); setMemberId(""); setTrainingId("");
  }

  async function handleDelete(id) { await trainingPlansApi.delete(id); load(); }

  const now = new Date();
  const active = plans.filter((p) => new Date(p.endDate) >= now).length;
  const expired = plans.filter((p) => new Date(p.endDate) < now).length;
  const avgDays = plans.length > 0 ? Math.round(plans.reduce((s, p) => s + (new Date(p.endDate) - new Date(p.startDate)) / (1000 * 60 * 60 * 24), 0) / plans.length) : 0;

  const trainingPopularity = trainings.map((t) => ({
    name: t.name.length > 12 ? t.name.substring(0, 12) + "…" : t.name,
    plans: plans.filter((p) => p.trainingId === t.id).length,
  })).filter((t) => t.plans > 0);

  const memberActivity = members.map((m) => ({
    name: `${m.firstName} ${m.lastName[0]}.`,
    plans: plans.filter((p) => p.memberId === m.id).length,
  })).filter((m) => m.plans > 0);

  return (
    <div className="entity-section">
      <div className="dashboard-stats">
        <div className="stat-card purple">
          <div className="stat-header"><span className="stat-label">Total Plans</span><div className="stat-icon-wrap purple">📋</div></div>
          <div className="stat-value">{plans.length}</div>
          <div className="stat-footer">All training plans</div>
        </div>
        <div className="stat-card green">
          <div className="stat-header"><span className="stat-label">Active Plans</span><div className="stat-icon-wrap green">✅</div></div>
          <div className="stat-value">{active}</div>
          <div className="stat-footer">Currently running</div>
        </div>
        <div className="stat-card blue">
          <div className="stat-header"><span className="stat-label">Expired Plans</span><div className="stat-icon-wrap blue">⏰</div></div>
          <div className="stat-value">{expired}</div>
          <div className="stat-footer">Completed plans</div>
        </div>
        <div className="stat-card orange">
          <div className="stat-header"><span className="stat-label">Avg Duration</span><div className="stat-icon-wrap orange">📅</div></div>
          <div className="stat-value">{avgDays}<span style={{ fontSize: "14px", color: "var(--text-muted)", marginLeft: "4px" }}>days</span></div>
          <div className="stat-footer">Per plan</div>
        </div>
      </div>

      <div className="page-grid">
        <div className="page-grid-main">
          <div className="section-header"><span className="section-title">Manage Plans</span><span className="record-count">{plans.length} records</span></div>
          <form className="entity-form" onSubmit={handleSubmit}>
            <div className="form-group"><label className="form-label">Member</label>
              <select className="form-select" value={memberId} onChange={(e) => setMemberId(e.target.value)} required>
                <option value="">Select member...</option>
                {members.map((m) => <option key={m.id} value={m.id}>{m.firstName} {m.lastName}</option>)}
              </select>
            </div>
            <div className="form-group"><label className="form-label">Training</label>
              <select className="form-select" value={trainingId} onChange={(e) => setTrainingId(e.target.value)} required>
                <option value="">Select training...</option>
                {trainings.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
            <div className="form-group"><label className="form-label">Start Date</label><input className="form-input" type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} required /></div>
            <div className="form-group"><label className="form-label">End Date</label><input className="form-input" type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} required /></div>
            <div className="form-actions">
              <button type="submit" className="btn btn-primary">{editingId ? "✓ Update" : "+ Add"}</button>
              {editingId && <button type="button" className="btn btn-ghost" onClick={handleCancelEdit}>Cancel</button>}
            </div>
          </form>
          {plans.length === 0 ? (
            <div className="table-wrapper"><div className="empty-state"><div className="empty-icon">📋</div><div className="empty-text">No plans yet</div><div className="empty-subtext">Add your first training plan above</div></div></div>
          ) : (
            <div className="table-wrapper">
              <table className="data-table">
                <thead><tr><th>ID</th><th>Member</th><th>Training</th><th>Period</th><th>Status</th><th>Actions</th></tr></thead>
                <tbody>
                  {plans.map((p) => {
                    const isActive = new Date(p.endDate) >= now;
                    const days = Math.round((new Date(p.endDate) - new Date(p.startDate)) / (1000 * 60 * 60 * 24));
                    return (
                      <tr key={p.id}>
                        <td><span className="cell-id">#{p.id}</span></td>
                        <td>{p.member ? <div className="cell-user"><div className="cell-avatar">{p.member.firstName[0]}{p.member.lastName[0]}</div><span className="cell-name">{p.member.firstName} {p.member.lastName}</span></div> : p.memberId}</td>
                        <td><span className="cell-badge">{p.training ? p.training.name : p.trainingId}</span></td>
                        <td><div><div className="cell-name">{new Date(p.startDate).toLocaleDateString()} — {new Date(p.endDate).toLocaleDateString()}</div><div className="cell-sub">{days} days</div></div></td>
                        <td><span className={`cell-status ${isActive ? "active" : "expired"}`}>{isActive ? "Active" : "Expired"}</span></td>
                        <td><div className="cell-actions"><button className="btn-icon" onClick={() => handleEdit(p)} title="Edit">✏️</button><button className="btn-icon danger" onClick={() => handleDelete(p.id)} title="Delete">🗑️</button></div></td>
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
            <div className="chart-title"><div className="chart-title-dot blue"></div> Popular Trainings</div>
            {trainingPopularity.length > 0 ? (
              <ResponsiveContainer width="100%" height={180}>
                <BarChart data={trainingPopularity} barSize={22} layout="vertical"><CartesianGrid strokeDasharray="3 3" stroke="#1a1a1f" /><XAxis type="number" tick={{ fill: "#505068", fontSize: 11 }} axisLine={false} tickLine={false} /><YAxis type="category" dataKey="name" tick={{ fill: "#505068", fontSize: 10 }} axisLine={false} tickLine={false} width={80} /><Tooltip contentStyle={tooltipStyle} cursor={{ fill: "rgba(108,92,231,0.06)" }} /><Bar dataKey="plans" fill="#60a5fa" radius={[0,6,6,0]} /></BarChart>
              </ResponsiveContainer>
            ) : (<div className="empty-state" style={{ padding: "24px" }}><div className="empty-subtext">Add plans to see chart</div></div>)}
          </div>
          <div className="side-card">
            <div className="chart-title"><div className="chart-title-dot orange"></div> Member Activity</div>
            {memberActivity.length > 0 ? (
              <ResponsiveContainer width="100%" height={180}>
                <BarChart data={memberActivity} barSize={22} layout="vertical"><CartesianGrid strokeDasharray="3 3" stroke="#1a1a1f" /><XAxis type="number" tick={{ fill: "#505068", fontSize: 11 }} axisLine={false} tickLine={false} /><YAxis type="category" dataKey="name" tick={{ fill: "#505068", fontSize: 10 }} axisLine={false} tickLine={false} width={80} /><Tooltip contentStyle={tooltipStyle} cursor={{ fill: "rgba(108,92,231,0.06)" }} /><Bar dataKey="plans" fill="#fb923c" radius={[0,6,6,0]} /></BarChart>
              </ResponsiveContainer>
            ) : (<div className="empty-state" style={{ padding: "24px" }}><div className="empty-subtext">Add plans to see activity</div></div>)}
          </div>
        </div>
      </div>
    </div>
  );
}

export default TrainingPlans;
