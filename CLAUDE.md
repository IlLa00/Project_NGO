# Project NGO - CLAUDE.md

## 프로젝트 소개

이 프로젝트의 궁극적인 목표는 **NGO를 활용해 유니티 멀티플레이에 관한 지식 습득**으로, 이에 따른 **유니티 멀티플레이 포트폴리오 생성**이 목표입니다.

현재 개발자는 언리얼 멀티플레이에 관한 지식은 보유 중이나, 유니티 멀티플레이에 대한 경험은 많지 않은 상태입니다. 언리얼 멀티플레이 개념과 연결지어 설명하면 이해에 도움이 됩니다.

---

## 절대 규칙

1. **반드시 질문할 것** — 애매모호하거나 확실하지 않은 사항은 절대 유추하지 말고, 반드시 먼저 질문할 것.
2. **최적화 필수** — PC + 모바일 크로스 플랫폼 환경이므로, 모든 구현에서 성능 최적화를 반드시 고려할 것.
3. **주석 금지**

---

## 개발 조건

| 항목 | 내용 |
|---|---|
| 장르 | Co-op 웨이브 디펜스 (3D) |
| 인원 | 솔로 |
| 플랫폼 | **PC (Windows) + 모바일 (Android) 크로스 플랫폼** |
| 타겟 시장 | 국내 (한국) 게임사 취업 포트폴리오 |
| UI 시스템 | UGUI 단독 사용 |

---

## 게임 기획

### 게임 흐름
```
메인 메뉴 → 로비 (방 생성/참가/레디) → 인게임 → 결과 화면 → 로비 복귀
```

### 인게임 구성
- 2~4명 플레이어 협동 (Co-op)
- 5웨이브 구성, 마지막 웨이브는 보스
- 중앙 거점을 중심으로 사방에서 몬스터 스폰
- 플레이어 전원 사망 시 게임 오버

### 웨이브 구성

| 웨이브 | 몬스터 구성 | 특이사항 |
|---|---|---|
| 1 | 기본 10마리 | 튜토리얼 수준 |
| 2 | 기본 15마리 + 원거리 3마리 | |
| 3 | 기본 20마리 + 원거리 5마리 | 스폰 속도 증가 |
| 4 | 엘리트 몬스터 등장 | HP 3배 |
| 5 | 보스 1마리 + 기본 10마리 | 보스 특수 패턴 |

### 몬스터 종류
- 기본 돌격형 — 가장 많이 등장, 플레이어에게 돌진
- 원거리형 — 거리 유지하며 공격, 플레이어 분산 유도
- 보스 — 고HP + 특수 패턴, 마지막 웨이브 등장

### 플레이어 기본 스펙
- 이동 / 기본 공격 / 스킬 1개 / HP / 팀원 부활 시스템

---

## 기술 스택

### 네트워크
- **NGO (Netcode for GameObjects)** — 멀티플레이 핵심
- **Unity Gaming Services**
  - Lobby Service — 방 생성/참가/레디 관리
  - Relay Service — 모바일 NAT 중계 (크로스 플랫폼 필수)
- 구조: Host-Client (언리얼 Listen Server와 동일 개념)
  - HOST: 방장 클라이언트가 서버 역할 동시 수행, 몬스터 AI/스폰/웨이브 로직 전담
  - CLIENT: 입력 전송 및 화면 렌더링

### 렌더링
- **URP (Universal Render Pipeline)** — 프로젝트 초기 세팅

### UI
- **UGUI 단독 사용**
  - Screen Space Overlay — 플레이어 HUD, 웨이브 정보, 조이스틱(모바일)
  - World Space — 몬스터 HP바 (머리 위)
- **Canvas Scaler** — Scale With Screen Size, 해상도 자동 대응
- **Safe Area** — 노치/펀치홀 기기 대응 (Screen.safeArea)
- **DOTween** — UI 애니메이션 (HP바 감소, 화면 전환 페이드 등)

### 크로스 플랫폼 입력
- **New Input System** — PC/모바일 동시 지원
  - 모바일: 가상 조이스틱 + 터치 버튼 활성화
  - PC: 키보드/마우스 사용, 조이스틱 UI 비활성화
  - InputAction 기반으로 플랫폼 분기 처리

### 에셋 관리
- **Addressables** — 몬스터 프리팹/UI 에셋 동적 로드 및 해제

### 성능 최적화
- **Object Pooling (UnityEngine.Pool)** — 몬스터, 투사체, 파티클 이펙트

### 데이터 설계
- **ScriptableObject** — 몬스터 스탯, 웨이브 구성 데이터 (코드/데이터 분리)

### AI / 상태 관리
- **State Machine**
  - 몬스터 AI: Idle → Chase → Attack → Die
  - 게임 상태: Lobby → WaveReady → InWave → BossClear → GameOver

### 비동기 처리
- **UniTask** — Coroutine 대체, 로비 연결 대기/씬 로딩 처리

---

## NGO 핵심 구현 목록

| 항목 | 설명 | 언리얼 대응 개념 |
|---|---|---|
| NetworkVariable | 플레이어 HP, 위치, 웨이브 번호 동기화 | Replicated 프로퍼티 |
| ServerRpc | 공격/데미지 처리 요청 (클라 → 서버) | Server RPC |
| ClientRpc | 몬스터 사망 이펙트 등 전파 (서버 → 전체) | NetMulticast |
| NetworkObject Spawning | 몬스터는 HOST에서만 Spawn | Authority Spawn |
| Scene Management | 씬 전환 전체 클라이언트 동기화 | Travel / SeamlessTravel |

---

## 8주 로드맵

### 1주차 — 환경 세팅
- Unity 3D 프로젝트 생성 + URP 세팅
- NGO + UGS 패키지 설치 및 연동
- Relay + Lobby 기본 연결 동작 확인
- 프로젝트 폴더 구조 확정

### 2주차 — 로비 시스템
- 방 생성 / 방 참가 / 플레이어 레디 구현
- 씬 전환 (로비 → 인게임) 동기화
- UGUI 로비 화면 구성
- Canvas Scaler 기준 해상도 설정

### 3주차 — 플레이어 구현
- NetworkObject 플레이어 동기화 (위치, 애니메이션)
- New Input System 세팅
  - 모바일: 가상 조이스틱 + 공격 버튼
  - PC: 키보드(WASD) + 마우스 클릭
- 플랫폼별 UI 분기 처리

### 4주차 — 전투 시스템
- 공격 / 데미지 처리 (ServerRpc)
- HP NetworkVariable 동기화
- World Space 몬스터 HP바 (UGUI)
- DOTween HP바 감소 애니메이션
- 플레이어 사망 / 팀원 부활 처리

### 5주차 — 몬스터 시스템
- State Machine 기반 몬스터 AI (NavMesh + NetworkObject)
- Addressables 몬스터 프리팹 동적 로드
- Object Pool 몬스터 스폰/회수

### 6주차 — 웨이브 시스템
- ScriptableObject 웨이브 데이터 설계
- 웨이브 매니저 서버 동기화
- 보스 몬스터 특수 패턴 구현
- UniTask 웨이브 전환 카운트다운

### 7주차 — UI / 연출
- 인게임 HUD 전체 (HP, 스킬 쿨타임, 웨이브 정보, 점수)
- 모바일/PC 플랫폼별 UI 최종 조정
- Safe Area 노치 대응
- DOTween 화면 전환, 게임 오버/클리어 연출
- 결과 화면

### 8주차 — 최적화 + 빌드
- 모바일 실기기 테스트 (Android)
- PC 빌드 테스트 (Windows)
- NetworkObject 수 최적화, 틱레이트 조절
- Object Pool 튜닝, 메모리 프로파일링
- 최종 QA 및 빌드 마무리

---

## 면접 어필 포인트

```
NGO Host-Client 구조 이해       서버 권한 설계, 치트 방지 구조 이해
UGS Lobby + Relay 경험          실무 즉시 투입 가능한 UGS 경험
크로스 플랫폼 (PC + 모바일)     플랫폼별 입력 분기 및 반응형 UI 설계 경험
Addressables 적용               에셋 메모리 관리 이해
Object Pooling                  모바일 GC 최적화 실전 경험
ScriptableObject 설계           데이터/코드 분리, 유지보수성 향상
State Machine AI                몬스터 행동 패턴 설계 능력
모바일 + PC 실기기 테스트       신뢰도 있는 완성도 증명
```
