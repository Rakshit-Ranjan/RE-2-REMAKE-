using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;

/** 
 * TO DO LIST -:
 * 1) RENAME VARIABLES
 * 2) ADD RECOIL
 * 3) MAKE THIS SYSTEM WITH SCRIPTABLE OBJECTS
 * 4) 
 * **/
namespace RPG {

	public class Bullet {
		public float time;
		public Vector3 initialPos;
		public Vector3 initialVel;
		public TrailRenderer tracer;
	}

    public class RaycastWeapon : MonoBehaviour {

		public PlayerCharacterManager player;
        public Transform rayCastOrigin;
        public TrailRenderer trail;
        public Transform crossHairTarget;
        public  ParticleSystem[] weaponMuzzleFX;
        public ParticleSystem weaponHitEffect;
		public Cinemachine.CinemachineFreeLook freeLookCam;

		[Header("Fire variables 🔥")]
		public int fireRate = 25;
		public bool isFiring; 
		//not in use rn
		public float bulletSpeed = 1000f;
		public float bulletDrop = 0.0f;
		public float bulletMaxLifetime = 3f;
		public float accumulatedTime;

		[Header("Recoil Variables")]
		public float verticalRecoil;
		public float horizontalRecoil;
		public float recoilduration;

		[Range(0, 1)]
		public float recoilEffect;

		Ray ray;
		RaycastHit hit;
		float recoilTime;
		public List<Bullet> bullets = new List<Bullet>();

		Vector3 GetPos(Bullet bullet) {
			// p + v*t + 0.5*g*t*t
			Vector3 g = Vector3.down * bulletDrop;
			return (bullet.initialPos) + (bullet.initialVel * bullet.time) + (0.5f * bullet.time * bullet.time * g);
		}

		Bullet CreateBullet(Vector3 pos, Vector3 vel) {
			Bullet b = new Bullet {
				initialPos = pos,
				initialVel = vel,
				time = 0.0f,
				tracer = Instantiate(trail, pos, Quaternion.identity)
			};
			b.tracer.AddPosition(pos);
			return b;
		}

		private void Start() {
			player = GetComponentInParent<PlayerCharacterManager>();
		}

		private void Update() {
			if(recoilTime > 0 && player.isShooting) {
			isFiring = true;
				freeLookCam.m_XAxis.Value -= ((Random.Range(-horizontalRecoil, horizontalRecoil) * Time.deltaTime) * 10 / recoilduration) * recoilEffect;
				freeLookCam.m_YAxis.Value -= ((Random.Range(-verticalRecoil, verticalRecoil) * Time.deltaTime) / (recoilduration *10)) * recoilEffect;
				recoilTime -= Time.deltaTime;
			}
		}

		public void StartFiring() {
			player.readyToShoot = false;
			FireSingleBullet();
			float interval = 1f / fireRate;
			Invoke(nameof(ResetShoot), interval);
		}

		#region buggy not in use....
		public void UpdateBullets(float delta) {
			SimulateBullet(delta);
			DestroyBullets();
		}

		void SimulateBullet(float delta) {
			bullets.ForEach(b => {
				Vector3 p0 = GetPos(b);
				b.time += delta;
				Vector3 p1 = GetPos(b);
				RaycastSegment(p0, p1, b);
			});
		}

		void DestroyBullets() {
			bullets.RemoveAll(b => b.time >= bulletMaxLifetime);
		}

		void RaycastSegment(Vector3 start, Vector3 end, Bullet bullet) {
			Vector3 dir = end - start;
			float dist = dir.magnitude;
			ray.origin = start;
			ray.direction = dir;
			if (Physics.Raycast(ray, out hit, dist)) {
				var hitFX = Instantiate(weaponHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
				hitFX.Emit(1);
				if (bullet.tracer != null) {
					bullet.tracer.transform.position = hit.point;
					bullet.time = bulletMaxLifetime;
				}
			} else {
				if(bullet.tracer != null)
					bullet.tracer.transform.position = end;
			}
		}
		#endregion
		
		void FireSingleBullet() {
			foreach (var p in weaponMuzzleFX) {
				p.Emit(1);
			}
			/**Vector3 velocity = (crossHairTarget.position - ray.origin).normalized * bulletSpeed;
			var bullet = CreateBullet(rayCastOrigin.position, velocity);
			bullets.Add(bullet);**/

			ray.origin = rayCastOrigin.position;
			ray.direction = crossHairTarget.position - rayCastOrigin.position;
			var tracer = Instantiate(trail, ray.origin, Quaternion.identity);
			tracer.AddPosition(ray.origin);
			if (Physics.Raycast(ray, out hit)) {
				var hitFX = Instantiate(weaponHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
				hitFX.Emit(1);
				tracer.transform.position = hit.point;
			}
			GenerateRecoil();
		}

		void GenerateRecoil() {
			recoilTime = recoilduration;
		}

		void ResetShoot() {
			player.readyToShoot = true;
		}

    }
}