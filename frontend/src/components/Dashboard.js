import { useState, useEffect } from "react";
import { trainersApi, trainingsApi, membersApi, trainingPlansApi } from "../services/api";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, AreaChart, Area, CartesianGrid } from "recharts";

const COLORS = ["#6c5ce7", "#34d399", "#60a5fa", "#fb923c", "#f472b6", "#a78bfa"];
const tooltipStyle = { backgroundColor: "#18181c", border: "1px solid #2a2a30", borderRadius: "10px", color: "#eeeef0", fontSize: "13px", padding: "8px 12px" };

function Dashboard() {
  const [trainers, setTrainers] = useState([]);
  const [trainings, setTrainings] = useState([]);
  const [members, setMembers] = useState([]);
  const [plans, setPlans] = useState([]);

  useEffect(() => {
    trainersApi.getAll().then(setTrainers).catch(() => setTrainers([]));
    trainingsApi.getAll().then(setTrainings).catch(() => setTrainings([]));
    membersApi.getAll().then(setMembers).catch(() => setMembers([]));
    trainingPlansApi.getAll().then(setPlans).catch(() => setPlans([]));
  }, []);

  const trainerWorkload = trainers.map((t) => ({
    name: `${t.firstName} ${t.lastName[0]}.`,
    trainings: trainings.filter((tr) => tr.trainerId === t.id).length,
    plans: plans.filter((p) => trainings.some((tr) => tr.trainerId === t.id && tr.id === p.trainingId)).length,
  }));

  const specializationData = trainers.reduce((acc, t) => {
    const existing = acc.find((item) => item.name === t.specialization);
    if (existing) existing.value += 1;
    else acc.push({ name: t.specialization || "Unspecified", value: 1 });
    return acc;
  }, []);

  const durationData = trainings.map((t) => ({
    name: t.name.length > 12 ? t.name.substring(0, 12) + "..." : t.name,
    duration: t.durationInMinutes,
  }));

  const monthlyData = (() => {
    const now = new Date();
    return Array.from({ length: 6 }, (_, i) => {
      const d = new Date(now.getFullYear(), now.getMonth() - 5 + i, 1);
      return {
        month: d.toLocaleString("en-US", { month: "short" }),
        members: members.filter((m) => {
          const joined = new Date(m.joinDate);
          return joined.getFullYear() === d.getFullYear() && joined.getMonth() === d.getMonth();
        }).length,
      };
    });
  })();

  const totalSessions = trainings.reduce((sum, t) => sum + t.durationInMinutes, 0);
  const avgDuration = trainings.length > 0 ? Math.round(totalSessions / trainings.length) : 0;

  return (
    <div className="entity-section">
      <div className="dashboard-stats">
        <div className="stat-card purple">
          <div className="stat-header"><span className="stat-label">Total Trainers</span><div className="stat-icon-wrap purple">⚡</div></div>
          <div className="stat-value">{trainers.length}</div>
          <div className="stat-footer"><span className="stat-trend">Active</span> professionals</div>
        </div>
        <div className="stat-card green">
          <div className="stat-header"><span className="stat-label">Total Trainings</span><div className="stat-icon-wrap green">🏋️</div></div>
          <div className="stat-value">{trainings.length}</div>
          <div className="stat-footer">Avg <span className="stat-trend">{avgDuration} min</span> per session</div>
        </div>
        <div className="stat-card blue">
          <div className="stat-header"><span className="stat-label">Total Members</span><div className="stat-icon-wrap blue">👥</div></div>
          <div className="stat-value">{members.length}</div>
          <div className="stat-footer"><span className="stat-trend">Registered</span> members</div>
        </div>
        <div className="stat-card orange">
          <div className="stat-header"><span className="stat-label">Active Plans</span><div className="stat-icon-wrap orange">📋</div></div>
          <div className="stat-value">{plans.length}</div>
          <div className="stat-footer"><span className="stat-trend">{totalSessions}</span> total minutes</div>
        </div>
      </div>
      <div className="charts-grid">
        <div className="chart-card">
          <div className="chart-title"><div className="chart-title-dot purple"></div> Member Growth</div>
          <ResponsiveContainer width="100%" height={220}>
            <AreaChart data={monthlyData}>
              <defs><linearGradient id="memberGrad" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stopColor="#6c5ce7" stopOpacity={0.3} /><stop offset="100%" stopColor="#6c5ce7" stopOpacity={0} /></linearGradient></defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#1a1a1f" /><XAxis dataKey="month" tick={{ fill: "#505068", fontSize: 12 }} axisLine={false} tickLine={false} /><YAxis tick={{ fill: "#505068", fontSize: 12 }} axisLine={false} tickLine={false} /><Tooltip contentStyle={tooltipStyle} /><Area type="monotone" dataKey="members" stroke="#6c5ce7" strokeWidth={2} fill="url(#memberGrad)" />
            </AreaChart>
          </ResponsiveContainer>
        </div>
        <div className="chart-card">
          <div className="chart-title"><div className="chart-title-dot green"></div> Specializations</div>
          {specializationData.length > 0 ? (
            <ResponsiveContainer width="100%" height={220}>
              <PieChart><Pie data={specializationData} cx="50%" cy="50%" innerRadius={55} outerRadius={85} paddingAngle={4} dataKey="value" stroke="none">{specializationData.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}</Pie><Tooltip contentStyle={tooltipStyle} /></PieChart>
            </ResponsiveContainer>
          ) : (<div className="empty-state"><div className="empty-icon">📈</div><div className="empty-subtext">Add trainers to see data</div></div>)}
        </div>
        <div className="chart-card">
          <div className="chart-title"><div className="chart-title-dot blue"></div> Training Duration</div>
          {durationData.length > 0 ? (
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={durationData} barSize={28}><CartesianGrid strokeDasharray="3 3" stroke="#1a1a1f" /><XAxis dataKey="name" tick={{ fill: "#505068", fontSize: 11 }} axisLine={false} tickLine={false} /><YAxis tick={{ fill: "#505068", fontSize: 12 }} axisLine={false} tickLine={false} /><Tooltip contentStyle={tooltipStyle} cursor={{ fill: "rgba(108,92,231,0.06)" }} /><Bar dataKey="duration" fill="#60a5fa" radius={[6,6,0,0]} /></BarChart>
            </ResponsiveContainer>
          ) : (<div className="empty-state"><div className="empty-icon">📊</div><div className="empty-subtext">Add trainings to see data</div></div>)}
        </div>
        <div className="chart-card">
          <div className="chart-title"><div className="chart-title-dot orange"></div> Trainer Workload</div>
          {trainerWorkload.length > 0 ? (
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={trainerWorkload} barSize={20}><CartesianGrid strokeDasharray="3 3" stroke="#1a1a1f" /><XAxis dataKey="name" tick={{ fill: "#505068", fontSize: 11 }} axisLine={false} tickLine={false} /><YAxis tick={{ fill: "#505068", fontSize: 12 }} axisLine={false} tickLine={false} /><Tooltip contentStyle={tooltipStyle} cursor={{ fill: "rgba(108,92,231,0.06)" }} /><Bar dataKey="trainings" fill="#6c5ce7" radius={[6,6,0,0]} name="Trainings" /><Bar dataKey="plans" fill="#fb923c" radius={[6,6,0,0]} name="Plans" /></BarChart>
            </ResponsiveContainer>
          ) : (<div className="empty-state"><div className="empty-icon">📉</div><div className="empty-subtext">Add data to see workload</div></div>)}
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
